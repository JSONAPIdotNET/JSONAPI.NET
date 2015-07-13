using System;
using System.Collections.Generic;
using JSONAPI.Core;
using JSONAPI.Documents;
using JSONAPI.Http;

namespace JSONAPI.Configuration
{
    /// <summary>
    /// Base class for JSON API configuration services
    /// </summary>
    public class JsonApiConfiguration : IJsonApiConfiguration
    {
        private readonly IResourceTypeRegistrar _resourceTypeRegistrar;
        public ILinkConventions LinkConventions { get; private set; }
        public IEnumerable<IResourceTypeConfiguration> ResourceTypeConfigurations { get { return _resourceTypeConfigurations; } }

        private readonly IList<IResourceTypeConfiguration> _resourceTypeConfigurations;

        /// <summary>
        /// Creates a new JsonApiConfiguration
        /// </summary>
        public JsonApiConfiguration()
            : this(new PluralizationService())
        {
        }

        /// <summary>
        /// Creates a new JsonApiConfiguration
        /// </summary>
        public JsonApiConfiguration(IPluralizationService pluralizationService)
            : this(new DefaultNamingConventions(pluralizationService))
        {
        }

        /// <summary>
        /// Creates a new JsonApiConfiguration
        /// </summary>
        public JsonApiConfiguration(INamingConventions namingConventions)
            : this(new ResourceTypeRegistrar(namingConventions))
        {
        }

        /// <summary>
        /// Creates a new JsonApiConfiguration
        /// </summary>
        public JsonApiConfiguration(IResourceTypeRegistrar resourceTypeRegistrar)
        {
            _resourceTypeRegistrar = resourceTypeRegistrar;
            if (resourceTypeRegistrar == null) throw new ArgumentNullException("resourceTypeRegistrar");

            _resourceTypeConfigurations = new List<IResourceTypeConfiguration>();
            LinkConventions = new DefaultLinkConventions();
        }

        /// <summary>
        /// Registers a resource type with the configuration
        /// </summary>
        public void RegisterResourceType<TResourceType>(Action<IResourceTypeConfigurator<TResourceType>> configurationAction = null)
        {
            var configuration = new ResourceTypeConfiguration<TResourceType>(_resourceTypeRegistrar);
            if (configurationAction != null)
                configurationAction(configuration);
            _resourceTypeConfigurations.Add(configuration);
        }

        /// <summary>
        /// Registers an entity type/resource type pair for use with MappedDocumentMaterializer />
        /// </summary>
        public void RegisterMappedType<TEntity, TResourceType, TMaterializer>(Action<IResourceTypeConfigurator<TResourceType>> configurationAction = null)
            where TMaterializer : MappedDocumentMaterializer<TEntity, TResourceType>
            where TResourceType : class
        {
            RegisterResourceType<TResourceType>(c =>
            {
                c.UseDocumentMaterializer<TMaterializer>();
                if (configurationAction != null)
                    configurationAction(c);
            });
        }

        /// <summary>
        /// Allows overriding how links will be formatted.
        /// </summary>
        /// <param name="linkConventions"></param>
        public void OverrideLinkConventions(ILinkConventions linkConventions)
        {
            LinkConventions = linkConventions;
        }
    }

    /// <summary>
    /// Configuration interface for JSON API
    /// </summary>
    public interface IJsonApiConfiguration
    {
        /// <summary>
        /// Conventions for serializing links with resource objects
        /// </summary>
        ILinkConventions LinkConventions { get; }

        /// <summary>
        /// A set of resource type configurations. These configurations will be converted into IResourceTypeRegistrations
        /// by the ResourceTypeRegistrar
        /// </summary>
        IEnumerable<IResourceTypeConfiguration> ResourceTypeConfigurations { get; }
    }
}
