using System;
using JSONAPI.Http;

namespace JSONAPI.Configuration
{
    /// <summary>
    /// Allows configuring a relationship
    /// </summary>
    public sealed class ResourceTypeRelationshipConfiguration : IResourceTypeRelationshipConfiguration, IResourceTypeRelationshipConfigurator
    {
        internal ResourceTypeRelationshipConfiguration()
        {
        }

        public Type MaterializerType { get; private set; }

        public void UseMaterializer<TMaterializerType>()
            where TMaterializerType : IRelatedResourceDocumentMaterializer
        {
            MaterializerType = typeof (TMaterializerType);
        }
    }
}