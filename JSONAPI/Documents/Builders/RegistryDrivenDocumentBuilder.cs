using System.Collections.Generic;
using System.Linq;
using JSONAPI.Core;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Documents.Builders
{
    /// <summary>
    /// Base class for the main document builders
    /// </summary>
    public abstract class RegistryDrivenDocumentBuilder
    {
        private readonly IResourceTypeRegistry _resourceTypeRegistry;
        private readonly ILinkConventions _linkConventions;

        /// <summary>
        /// Creates a new RegistryDrivenDocumentBuilder
        /// </summary>
        /// <param name="resourceTypeRegistry"></param>
        /// <param name="linkConventions"></param>
        protected RegistryDrivenDocumentBuilder(IResourceTypeRegistry resourceTypeRegistry, ILinkConventions linkConventions)
        {
            _resourceTypeRegistry = resourceTypeRegistry;
            _linkConventions = linkConventions;
        }

        internal static bool PathExpressionMatchesCurrentPath(string currentPath, string pathToInclude)
        {
            if (string.IsNullOrEmpty(pathToInclude)) return false;
            if (currentPath == pathToInclude) return true;

            var currentPathSegments = currentPath.Split('.');
            var pathToIncludeSegments = pathToInclude.Split('.');

            // Same number of segments fails because we already checked for equality above
            if (currentPathSegments.Length >= pathToIncludeSegments.Length) return false;

            return !currentPathSegments.Where((t, i) => t != pathToIncludeSegments[i]).Any();
        }

        /// <summary>
        /// Creates a JSON API resource object from the given CLR object
        /// </summary>
        /// <param name="modelObject"></param>
        /// <param name="idDictionariesByType"></param>
        /// <param name="currentPath"></param>
        /// <param name="includePathExpressions"></param>
        /// <param name="linkBaseUrl"></param>
        /// <returns></returns>
        protected ResourceObject CreateResourceObject(object modelObject, IDictionary<string, IDictionary<string, ResourceObject>> idDictionariesByType,
            string currentPath, string[] includePathExpressions, string linkBaseUrl)
        {
            if (modelObject == null) return null;

            var modelObjectRuntimeType = modelObject.GetType();
            var resourceTypeRegistration = _resourceTypeRegistry.GetRegistrationForType(modelObjectRuntimeType);

            var attributes = new Dictionary<string, JToken>();
            var relationships = new Dictionary<string, IRelationshipObject>();

            foreach (var attribute in resourceTypeRegistration.Attributes)
            {
                var propertyValue = attribute.GetValue(modelObject);
                attributes[attribute.JsonKey] = propertyValue;
            }

            foreach (var modelRelationship in resourceTypeRegistration.Relationships)
            {
                IResourceLinkage linkage = null;

                var childPath = currentPath == null
                    ? modelRelationship.JsonKey
                    : (currentPath + "." + modelRelationship.JsonKey);
                if (includePathExpressions != null &&
                    includePathExpressions.Any(e => PathExpressionMatchesCurrentPath(childPath, e)))
                {
                    if (modelRelationship.IsToMany)
                    {
                        var propertyValue =
                            (IEnumerable<object>)modelRelationship.Property.GetValue(modelObject);
                        if (propertyValue != null)
                        {
                            var identifiers = new List<IResourceIdentifier>();
                            foreach (var relatedResource in propertyValue)
                            {
                                var identifier = GetResourceIdentifierForResource(relatedResource);
                                identifiers.Add(identifier);

                                IDictionary<string, ResourceObject> idDictionary;
                                if (!idDictionariesByType.TryGetValue(identifier.Type, out idDictionary))
                                {
                                    idDictionary = new Dictionary<string, ResourceObject>();
                                    idDictionariesByType[identifier.Type] = idDictionary;
                                }

                                ResourceObject relatedResourceObject;
                                if (!idDictionary.TryGetValue(identifier.Id, out relatedResourceObject))
                                {
                                    relatedResourceObject = CreateResourceObject(relatedResource, idDictionariesByType,
                                        childPath, includePathExpressions, linkBaseUrl);
                                    idDictionary[identifier.Id] = relatedResourceObject;
                                }
                            }
                            linkage = new ToManyResourceLinkage(identifiers.ToArray());
                        }
                    }
                    else
                    {
                        var relatedResource = modelRelationship.Property.GetValue(modelObject);
                        if (relatedResource != null)
                        {
                            var identifier = GetResourceIdentifierForResource(relatedResource);

                            IDictionary<string, ResourceObject> idDictionary;
                            if (!idDictionariesByType.TryGetValue(identifier.Type, out idDictionary))
                            {
                                idDictionary = new Dictionary<string, ResourceObject>();
                                idDictionariesByType[identifier.Type] = idDictionary;
                            }

                            ResourceObject relatedResourceObject;
                            if (!idDictionary.TryGetValue(identifier.Id, out relatedResourceObject))
                            {
                                relatedResourceObject = CreateResourceObject(relatedResource, idDictionariesByType,
                                    childPath, includePathExpressions, linkBaseUrl);
                                idDictionary[identifier.Id] = relatedResourceObject;
                            }

                            linkage = new ToOneResourceLinkage(identifier);
                        }
                        else
                        {
                            linkage = new ToOneResourceLinkage(null);
                        }
                    }
                }

                var selfLink = _linkConventions.GetRelationshipLink(modelObject, _resourceTypeRegistry, modelRelationship, linkBaseUrl);
                var relatedResourceLink = _linkConventions.GetRelatedResourceLink(modelObject, _resourceTypeRegistry, modelRelationship, linkBaseUrl);

                relationships[modelRelationship.JsonKey] = new RelationshipObject(linkage, selfLink, relatedResourceLink);
            }

            var resourceId = resourceTypeRegistration.GetIdForResource(modelObject);
            return new ResourceObject(resourceTypeRegistration.ResourceTypeName, resourceId, attributes, relationships);
        }

        /// <summary>
        /// Creates a JSON API resource identifier that points to the given resource instance
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        protected ResourceIdentifier GetResourceIdentifierForResource(object resource)
        {
            var relatedResourceType = resource.GetType();
            var relatedResourceRegistration = _resourceTypeRegistry.GetRegistrationForType(relatedResourceType);
            var relatedResourceTypeName = relatedResourceRegistration.ResourceTypeName;
            var relatedResourceId = relatedResourceRegistration.GetIdForResource(resource);
            return new ResourceIdentifier(relatedResourceTypeName, relatedResourceId);
        }
    }
}