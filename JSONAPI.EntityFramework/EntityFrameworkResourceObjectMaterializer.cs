using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Core;
using JSONAPI.Json;
using JSONAPI.Payload;
using Newtonsoft.Json.Linq;

namespace JSONAPI.EntityFramework
{
    /// <summary>
    /// This class manages converting IResourceObject instances from a request into records managed
    /// by Entity Framework.
    /// </summary>
    public class EntityFrameworkEntityFrameworkResourceObjectMaterializer
    {
        private readonly DbContext _dbContext;
        private readonly IResourceTypeRegistry _registry;
        private readonly MethodInfo _openSetToManyRelationshipValueMethod;

        /// <summary>
        /// Creates a new EntityFrameworkEntityFrameworkResourceObjectMaterializer
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="registry"></param>
        public EntityFrameworkEntityFrameworkResourceObjectMaterializer(DbContext dbContext, IResourceTypeRegistry registry)
        {
            _dbContext = dbContext;
            _registry = registry;
            _openSetToManyRelationshipValueMethod = GetType()
                .GetMethod("SetToManyRelationshipValue", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Gets a record managed by Entity Framework that has merged in the data from
        /// the supplied resource object.
        /// </summary>
        /// <param name="resourceObject"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="DeserializationException"></exception>
        public async Task<object> MaterializeResourceObject(IResourceObject resourceObject, CancellationToken cancellationToken)
        {
            var registration = _registry.GetRegistrationForResourceTypeName(resourceObject.Type);

            var material = await GetExistingRecord(registration, resourceObject.Id, cancellationToken);
            if (material == null)
            {
                material = Activator.CreateInstance(registration.Type);
                registration.IdProperty.SetValue(material, resourceObject.Id);
                _dbContext.Set(registration.Type).Add(material);
            }

            await MergeFieldsIntoProperties(resourceObject, material, registration, cancellationToken);

            return material;
        }

        /// <summary>
        /// Gets an existing record from the store by ID, if it exists
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual Task<object> GetExistingRecord(IResourceTypeRegistration registration, string id, CancellationToken cancellationToken)
        {
            return _dbContext.Set(registration.Type).FindAsync(cancellationToken, id);
        }

        /// <summary>
        /// Merges the field values of the given resource object into the materialized object
        /// </summary>
        /// <param name="resourceObject"></param>
        /// <param name="material"></param>
        /// <param name="registration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="DeserializationException">Thrown when a semantically incorrect part of the payload is encountered</exception>
        protected virtual async Task MergeFieldsIntoProperties(IResourceObject resourceObject, object material,
            IResourceTypeRegistration registration, CancellationToken cancellationToken)
        {
            foreach (var attributeValue in resourceObject.Attributes)
            {
                var attribute = (ResourceTypeAttribute) registration.GetFieldByName(attributeValue.Key);
                attribute.SetValue(material, attributeValue.Value);
            }

            foreach (var relationshipValue in resourceObject.Relationships)
            {
                var linkage = relationshipValue.Value.Linkage;

                var typeRelationship = (ResourceTypeRelationship) registration.GetFieldByName(relationshipValue.Key);
                if (typeRelationship.IsToMany)
                {
                    if (linkage == null)
                        throw new DeserializationException("Missing linkage for to-many relationship",
                            "Expected an array for to-many linkage, but no linkage was specified.", "/data/relationships/" + relationshipValue.Key);

                    if (linkage.LinkageToken == null)
                        throw new DeserializationException("Null linkage for to-many relationship",
                            "Expected an array for to-many linkage, but got Null.",
                            "/data/relationships/" + relationshipValue.Key + "/data");

                    var linkageTokenType = linkage.LinkageToken.Type;
                    if (linkageTokenType != JTokenType.Array)
                        throw new DeserializationException("Invalid linkage for to-many relationship",
                            "Expected an array for to-many linkage, but got " + linkage.LinkageToken.Type,
                            "/data/relationships/" + relationshipValue.Key + "/data");

                    var linkageArray = (JArray) linkage.LinkageToken;
                    
                    // TODO: One query per related object is going to be slow. At the very least, we should be able to group the queries by type
                    var newCollection = new List<object>();
                    foreach (var resourceIdentifier in linkageArray)
                    {
                        var resourceIdentifierObject = (JObject) resourceIdentifier;
                        var relatedType = resourceIdentifierObject["type"].Value<string>();
                        var relatedId = resourceIdentifierObject["id"].Value<string>();
                        
                        var relatedObjectRegistration = _registry.GetRegistrationForResourceTypeName(relatedType);
                        var relatedObject = await GetExistingRecord(relatedObjectRegistration, relatedId, cancellationToken);
                        newCollection.Add(relatedObject);
                    }

                    var method = _openSetToManyRelationshipValueMethod.MakeGenericMethod(typeRelationship.RelatedType);
                    method.Invoke(this, new[] { material, newCollection, typeRelationship });
                }
                else
                {
                    if (linkage == null)
                        throw new DeserializationException("Missing linkage for to-one relationship",
                            "Expected an object for to-one linkage, but no linkage was specified.", "/data/relationships/" + relationshipValue.Key);

                    if (linkage.LinkageToken == null)
                    {
                        // For some reason we have to get the value first, or else setting it to null does nothing.
                        // TODO: This will cause a synchronous query. We can get rid of this line entirely by using Include when the object is first fetched.
                        typeRelationship.Property.GetValue(material);
                        typeRelationship.Property.SetValue(material, null);
                    }
                    else
                    {
                        var linkageTokenType = linkage.LinkageToken.Type;
                        if (linkageTokenType != JTokenType.Object)
                            throw new DeserializationException("Invalid linkage for to-one relationship",
                                "Expected an object for to-one linkage, but got " + linkage.LinkageToken.Type,
                                "/data/relationships/" + relationshipValue.Key + "/data");

                        var linkageObject = (JObject) linkage.LinkageToken;
                        var relatedType = linkageObject["type"].Value<string>();
                        var relatedId = linkageObject["id"].Value<string>();

                        var relatedObjectRegistration = _registry.GetRegistrationForResourceTypeName(relatedType);
                        var relatedObject =
                            await GetExistingRecord(relatedObjectRegistration, relatedId, cancellationToken);

                        typeRelationship.Property.SetValue(material, relatedObject);
                    }
                }
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void SetToManyRelationshipValue<TRelated>(object material, IEnumerable<object> relatedObjects, ResourceTypeRelationship relationship)
        {
            // TODO: we need to fetch this property asynchronously first
            var currentValue = relationship.Property.GetValue(material);
            var typedArray = relatedObjects.Select(o => (TRelated) o).ToArray();
            if (relationship.Property.PropertyType.IsAssignableFrom(typeof (List<TRelated>)))
            {
                if (currentValue == null)
                {
                    relationship.Property.SetValue(material, typedArray.ToList());
                }
                else
                {
                    var listCurrentValue = (ICollection<TRelated>) currentValue;
                    var itemsToAdd = typedArray.Except(listCurrentValue);
                    var itemsToRemove = listCurrentValue.Except(typedArray).ToList();

                    foreach (var related in itemsToAdd)
                        listCurrentValue.Add(related);

                    foreach (var related in itemsToRemove)
                        listCurrentValue.Remove(related);
                }
            }
            else
            {
                relationship.Property.SetValue(material, typedArray);
            }
        }
    }
}
