using System;

namespace JSONAPI.Http
{
    /// <summary>
    /// Service to lookup document materializers
    /// </summary>
    public interface IDocumentMaterializerLocator
    {
        /// <summary>
        /// Resolves a <see cref="IDocumentMaterializer"/> for the given resource type name.
        /// </summary>
        IDocumentMaterializer GetMaterializerByResourceTypeName(string resourceTypeName);

        /// <summary>
        /// Resolves a <see cref="IDocumentMaterializer"/> for the given type.
        /// </summary>
        IDocumentMaterializer GetMaterializerByType(Type type);

        /// <summary>
        /// Resolves a <see cref="IRelatedResourceDocumentMaterializer" /> for the given resource type and relationship.
        /// </summary>
        IRelatedResourceDocumentMaterializer GetRelatedResourceMaterializer(string resourceTypeName, string relationshipName);
    }
}
