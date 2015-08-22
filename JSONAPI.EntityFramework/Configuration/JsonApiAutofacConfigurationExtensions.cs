using System;
using JSONAPI.Configuration;
using JSONAPI.EntityFramework.Http;

namespace JSONAPI.EntityFramework.Configuration
{
    public static class JsonApiConfigurationExtensions
    {
        public static void RegisterEntityFrameworkResourceType<TResourceType>(this JsonApiConfiguration jsonApiConfiguration,
            Action<IResourceTypeConfigurator<TResourceType>> configurationAction = null) where TResourceType : class
        {
            jsonApiConfiguration.RegisterResourceType<TResourceType>(c =>
            {
                c.UseDocumentMaterializer<EntityFrameworkDocumentMaterializer<TResourceType>>();
                if (configurationAction != null)
                    configurationAction(c);
            });
        }
    }
}
