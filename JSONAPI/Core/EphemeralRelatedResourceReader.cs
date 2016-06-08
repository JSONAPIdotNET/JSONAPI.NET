using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JSONAPI.Documents;
using JSONAPI.Json;

namespace JSONAPI.Core
{
    /// <summary>
    /// Populates property values on an ephemeral resource
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete]
    public class EphemeralRelatedResourceReader<T> : IEphemeralRelatedResourceReader<T>
    {
        private readonly IEphemeralRelatedResourceReader _ephemeralRelatedResourceReader;

        /// <summary>
        /// Creates a new EphemeralRelatedResourceReader
        /// </summary>
        /// <param name="ephemeralRelatedResourceReader"></param>
        public EphemeralRelatedResourceReader(IEphemeralRelatedResourceReader ephemeralRelatedResourceReader)
        {
            _ephemeralRelatedResourceReader = ephemeralRelatedResourceReader;
        }

        public void SetProperty(T ephemeralResource, string jsonKey, IRelationshipObject relationshipObject)
        {
            _ephemeralRelatedResourceReader.SetProperty(ephemeralResource, jsonKey, relationshipObject);
        }
    }

    /// <summary>
    /// Populates property values on an ephemeral resource
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EphemeralRelatedResourceReader : IEphemeralRelatedResourceReader
    {
        private readonly IResourceTypeRegistry _resourceTypeRegistry;
        private readonly IEphemeralRelatedResourceCreator _ephemeralRelatedResourceCreator;
        private readonly MethodInfo _openSetToManyRelationshipValueMethod;

        /// <summary>
        /// Creates a new EphemeralRelatedResourceReader
        /// </summary>
        /// <param name="resourceTypeRegistry"></param>
        /// <param name="ephemeralRelatedResourceCreator"></param>
        public EphemeralRelatedResourceReader(IResourceTypeRegistry resourceTypeRegistry, IEphemeralRelatedResourceCreator ephemeralRelatedResourceCreator)
        {
            _resourceTypeRegistry = resourceTypeRegistry;
            _ephemeralRelatedResourceCreator = ephemeralRelatedResourceCreator;
            _openSetToManyRelationshipValueMethod = GetType()
                .GetMethod("SetToManyRelationshipValue", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void SetProperty<T>(T ephemeralResource, string jsonKey, IRelationshipObject relationshipObject)
        {
            var resourceTypeRegistration = new Lazy<IResourceTypeRegistration>(() => _resourceTypeRegistry.GetRegistrationForType(typeof(T)));
            var relationship = resourceTypeRegistration.Value.GetFieldByName(jsonKey) as ResourceTypeRelationship;
            if (relationship == null) return;

            if (relationship.IsToMany)
                SetPropertyForToManyRelationship(ephemeralResource, relationship, relationshipObject.Linkage);
            else
                SetPropertyForToOneRelationship(ephemeralResource, relationship, relationshipObject.Linkage);
        }

        protected virtual void SetPropertyForToOneRelationship<T>(T ephemeralResource, ResourceTypeRelationship relationship, IResourceLinkage linkage)
        {
            if (linkage == null)
                throw new DeserializationException("Missing linkage for to-one relationship",
                    "Expected an object for to-one linkage, but no linkage was specified.", $"/data/relationships/{relationship.JsonKey}");

            if (linkage.IsToMany)
                throw new DeserializationException("Invalid linkage for to-one relationship",
                    "Expected an object or null for to-one linkage",
                    $"/data/relationships/{relationship.JsonKey}/data");

            var identifier = linkage.Identifiers.FirstOrDefault();
            if (identifier == null)
            {
                relationship.Property.SetValue(ephemeralResource, null);
            }
            else
            {
                var relatedObjectRegistration = _resourceTypeRegistry.GetRegistrationForResourceTypeName(identifier.Type);
                var relatedObject = _ephemeralRelatedResourceCreator.CreateEphemeralResource(relatedObjectRegistration, identifier.Id);

                relationship.Property.SetValue(ephemeralResource, relatedObject);
            }
        }

        protected virtual void SetPropertyForToManyRelationship<T>(T ephemeralResource, ResourceTypeRelationship relationship,
            IResourceLinkage linkage)
        {
            if (linkage == null)
                throw new DeserializationException("Missing linkage for to-many relationship",
                    "Expected an array for to-many linkage, but no linkage was specified.", $"/data/relationships/{relationship.JsonKey}");

            if (!linkage.IsToMany)
                throw new DeserializationException("Invalid linkage for to-many relationship",
                    "Expected an array for to-many linkage.",
                    $"/data/relationships/{relationship.JsonKey}/data");

            var newCollection = (from resourceIdentifier in linkage.Identifiers
                                 let relatedObjectRegistration = _resourceTypeRegistry.GetRegistrationForResourceTypeName(resourceIdentifier.Type)
                                 select _ephemeralRelatedResourceCreator.CreateEphemeralResource(relatedObjectRegistration, resourceIdentifier.Id)).ToList();

            var method = _openSetToManyRelationshipValueMethod.MakeGenericMethod(relationship.RelatedType);
            method.Invoke(this, new object[] { ephemeralResource, newCollection, relationship });
        }

        /// <summary>
        /// Sets the value of a to-many relationship
        /// </summary>
        protected void SetToManyRelationshipValue<TRelated>(object material, IEnumerable<object> relatedObjects, ResourceTypeRelationship relationship)
        {
            var currentValue = relationship.Property.GetValue(material);
            var typedArray = relatedObjects.Select(o => (TRelated)o).ToArray();
            if (relationship.Property.PropertyType.IsAssignableFrom(typeof(List<TRelated>)))
            {
                if (currentValue == null)
                {
                    relationship.Property.SetValue(material, typedArray.ToList());
                }
                else
                {
                    var listCurrentValue = (ICollection<TRelated>)currentValue;
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
