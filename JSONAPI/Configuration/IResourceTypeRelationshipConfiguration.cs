using System;
using JSONAPI.Http;

namespace JSONAPI.Configuration
{
    /// <summary>
    /// Results of configuring a relationship
    /// </summary>
    public interface IResourceTypeRelationshipConfiguration
    {
        /// <summary>
        /// The <see cref="IRelatedResourceDocumentMaterializer"/> type to use for materializing this relationship
        /// </summary>
        Type MaterializerType { get; }
    }
}