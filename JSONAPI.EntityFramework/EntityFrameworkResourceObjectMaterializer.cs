using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Core;
using JSONAPI.Documents;
using JSONAPI.Json;

namespace JSONAPI.EntityFramework
{
    /// <summary>
    /// Default implementation of IEntityFrameworkResourceObjectMaterializer
    /// </summary>
    public class EntityFrameworkResourceObjectMaterializer : IEntityFrameworkResourceObjectMaterializer
    {
        private readonly DbContext _dbContext;
        private readonly IResourceTypeRegistry _registry;
        private readonly MethodInfo _openSetToManyRelationshipValueMethod;
        private readonly MethodInfo _openGetExistingRecordGenericMethod;

        /// <summary>
        /// Creates a new EntityFrameworkEntityFrameworkResourceObjectMaterializer
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="registry"></param>
        public EntityFrameworkResourceObjectMaterializer(DbContext dbContext, IResourceTypeRegistry registry)
        {
            _dbContext = dbContext;
            _registry = registry;
            _openSetToManyRelationshipValueMethod = GetType()
                .GetMethod("SetToManyRelationshipValue", BindingFlags.NonPublic | BindingFlags.Instance);
            _openGetExistingRecordGenericMethod = GetType()
                .GetMethod("GetExistingRecordGeneric", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public async Task<object> MaterializeResourceObject(IResourceObject resourceObject, CancellationToken cancellationToken)
        {
            var registration = _registry.GetRegistrationForResourceTypeName(resourceObject.Type);
            
            var relationshipsToInclude = new List<ResourceTypeRelationship>();
            if (resourceObject.Relationships != null)
            {
                relationshipsToInclude.AddRange(
                    resourceObject.Relationships
                        .Select(relationshipObject => registration.GetFieldByName(relationshipObject.Key))
                        .OfType<ResourceTypeRelationship>());
            }


            var material = await GetExistingRecord(registration, resourceObject.Id, relationshipsToInclude.ToArray(), cancellationToken);
            if (material == null)
            {
                material = Activator.CreateInstance(registration.Type);
                await SetIdForNewResource(resourceObject, material, registration);
                _dbContext.Set(registration.Type).Add(material);
            }

            await MergeFieldsIntoProperties(resourceObject, material, registration, cancellationToken);

            return material;
        }

        /// <summary>
        /// Allows implementers to control how a new resource's ID should be set.
        /// </summary>
        protected virtual Task SetIdForNewResource(IResourceObject resourceObject, object newObject, IResourceTypeRegistration typeRegistration)
        {
            typeRegistration.IdProperty.SetValue(newObject, resourceObject.Id);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Gets an existing record from the store by ID, if it exists
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<object> GetExistingRecord(IResourceTypeRegistration registration, string id,
            ResourceTypeRelationship[] relationshipsToInclude, CancellationToken cancellationToken)
        {
            var method = _openGetExistingRecordGenericMethod.MakeGenericMethod(registration.Type);
            var result = (dynamic) method.Invoke(this, new object[] {registration, id, relationshipsToInclude, cancellationToken});
            return await result;
        }

        /// <summary>
        /// Merges the field values of the given resource object into the materialized object
        /// </summary>
        /// <param name="resourceObject"></param>
        /// <param name="material"></param>
        /// <param name="registration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="DeserializationException">Thrown when a semantically incorrect part of the document is encountered</exception>
        protected virtual async Task MergeFieldsIntoProperties(IResourceObject resourceObject, object material,
            IResourceTypeRegistration registration, CancellationToken cancellationToken)
        {
            foreach (var attributeValue in resourceObject.Attributes)
            {
                var attribute = registration.GetFieldByName(attributeValue.Key) as ResourceTypeAttribute;
                if (attribute == null) continue;
                attribute.SetValue(material, attributeValue.Value);
            }

            foreach (var relationshipValue in resourceObject.Relationships)
            {
                var linkage = relationshipValue.Value.Linkage;

                var typeRelationship = registration.GetFieldByName(relationshipValue.Key) as ResourceTypeRelationship;
                if (typeRelationship == null) continue;

                if (typeRelationship.IsToMany)
                {
                    if (linkage == null)
                        throw new DeserializationException("Missing linkage for to-many relationship",
                            "Expected an array for to-many linkage, but no linkage was specified.", "/data/relationships/" + relationshipValue.Key);

                    if (!linkage.IsToMany)
                        throw new DeserializationException("Invalid linkage for to-many relationship",
                            "Expected an array for to-many linkage.",
                            "/data/relationships/" + relationshipValue.Key + "/data");

                    // TODO: One query per related object is going to be slow. At the very least, we should be able to group the queries by type
                    var newCollection = new List<object>();
                    foreach (var resourceIdentifier in linkage.Identifiers)
                    {
                        var relatedObjectRegistration = _registry.GetRegistrationForResourceTypeName(resourceIdentifier.Type);
                        var relatedObject = await GetExistingRecord(relatedObjectRegistration, resourceIdentifier.Id, null, cancellationToken);
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

                    if (linkage.IsToMany)
                        throw new DeserializationException("Invalid linkage for to-one relationship",
                            "Expected an object or null for to-one linkage",
                            "/data/relationships/" + relationshipValue.Key + "/data");

                    var identifier = linkage.Identifiers.FirstOrDefault();
                    if (identifier == null)
                    {
                        typeRelationship.Property.SetValue(material, null);
                    }
                    else
                    {
                        var relatedObjectRegistration = _registry.GetRegistrationForResourceTypeName(identifier.Type);
                        var relatedObject =
                            await GetExistingRecord(relatedObjectRegistration, identifier.Id, null, cancellationToken);

                        typeRelationship.Property.SetValue(material, relatedObject);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a record by ID
        /// </summary>
        protected async Task<TRecord> GetExistingRecordGeneric<TRecord>(IResourceTypeRegistration registration,
            string id, ResourceTypeRelationship[] relationshipsToInclude, CancellationToken cancellationToken) where TRecord : class
        {
            var param = Expression.Parameter(registration.Type);
            var filterExpression = registration.GetFilterByIdExpression(param, id);
            var lambda = Expression.Lambda<Func<TRecord, bool>>(filterExpression, param);
            var query = _dbContext.Set<TRecord>().AsQueryable()
                .Where(lambda);

            if (relationshipsToInclude != null)
            {
                query = relationshipsToInclude.Aggregate(query,
                    (current, resourceTypeRelationship) => current.Include(resourceTypeRelationship.Property.Name));
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Sets the value of a to-many relationship
        /// </summary>
        protected void SetToManyRelationshipValue<TRelated>(object material, IEnumerable<object> relatedObjects, ResourceTypeRelationship relationship)
        {
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
