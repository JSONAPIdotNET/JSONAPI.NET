using System;
using JSONAPI.Http;

namespace JSONAPI.Configuration
{
    /// <summary>
    /// Default implementation of <see cref="IResourceTypeRelationshipConfiguration"/>
    /// </summary>
    public sealed class ResourceTypeRelationshipConfiguration : IResourceTypeRelationshipConfiguration
    {
        internal ResourceTypeRelationshipConfiguration()
        {
        }

        public Type MaterializerType { get; private set; }

        /// <summary>
        /// Specify the materializer type to use for this particular relationship
        /// </summary>
        public void UseMaterializer<TMaterializerType>()
            where TMaterializerType : IRelatedResourceDocumentMaterializer
        {
            MaterializerType = typeof (TMaterializerType);
        }
    }

    /// <summary>
    /// Configuration mechanism for relationships
    /// </summary>
    public interface IResourceTypeRelationshipConfiguration
    {
        /// <summary>
        /// The <see cref="IRelatedResourceDocumentMaterializer"/> type to use for materializing this relationship
        /// </summary>
        Type MaterializerType { get; }
    }
}