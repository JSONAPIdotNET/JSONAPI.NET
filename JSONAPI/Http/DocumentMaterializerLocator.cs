using System;

namespace JSONAPI.Http
{
    /// <summary>
    /// Default implementation of <see cref="IDocumentMaterializerLocator"/>
    /// </summary>
    public class DocumentMaterializerLocator : IDocumentMaterializerLocator
    {
        private readonly Func<string, IDocumentMaterializer> _nameResolver;
        private readonly Func<Type, IDocumentMaterializer> _typeResolver;
        private readonly Func<string, string, IRelatedResourceDocumentMaterializer> _relatedResourceMaterializerResolver;

        /// <summary>
        /// Creates a new <see cref="DocumentMaterializerLocator"/>
        /// </summary>
        public DocumentMaterializerLocator(Func<string, IDocumentMaterializer> nameResolver,
            Func<Type, IDocumentMaterializer> typeResolver,
            Func<string, string, IRelatedResourceDocumentMaterializer> relatedResourceMaterializerResolver)
        {
            _nameResolver = nameResolver;
            _typeResolver = typeResolver;
            _relatedResourceMaterializerResolver = relatedResourceMaterializerResolver;
        }

        public IDocumentMaterializer GetMaterializerByResourceTypeName(string resourceTypeName)
        {
            return _nameResolver(resourceTypeName);
        }

        public IDocumentMaterializer GetMaterializerByType(Type type)
        {
            return _typeResolver(type);
        }

        public IRelatedResourceDocumentMaterializer GetRelatedResourceMaterializer(string resourceTypeName, string relationshipName)
        {
            return _relatedResourceMaterializerResolver(resourceTypeName, relationshipName);
        }
    }
}