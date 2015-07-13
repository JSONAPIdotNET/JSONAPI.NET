using System;
using System.Linq.Expressions;
using JSONAPI.Http;

namespace JSONAPI.Configuration
{
    /// <summary>
    /// Configuration mechanism for resource types
    /// </summary>
    public interface IResourceTypeConfigurator<TResourceType>
    {
        /// <summary>
        /// Configures the relationship corresponding to the specified property
        /// </summary>
        void ConfigureRelationship(Expression<Func<TResourceType, object>> property,
            Action<IResourceTypeRelationshipConfigurator> configurationAction);

        /// <summary>
        /// Specifies the materializer to use for this resource type
        /// </summary>
        /// <typeparam name="TMaterializer"></typeparam>
        void UseDocumentMaterializer<TMaterializer>() where TMaterializer : IDocumentMaterializer;
    }
}