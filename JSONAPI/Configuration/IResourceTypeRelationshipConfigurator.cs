using JSONAPI.Http;

namespace JSONAPI.Configuration
{
    /// <summary>
    /// Configuration mechanism for relationships
    /// </summary>
    public interface IResourceTypeRelationshipConfigurator
    {
        /// <summary>
        /// Specify the materializer type to use for this particular relationship
        /// </summary>
        void UseMaterializer<TMaterializerType>() where TMaterializerType : IRelatedResourceDocumentMaterializer;
    }
}