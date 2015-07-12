using System;
using JSONAPI.Configuration;
using JSONAPI.EntityFramework.Http;

namespace JSONAPI.Autofac.EntityFramework
{
    public static class JsonApiAutofacConfigurationExtensions
    {
        public static void RegisterEntityFrameworkResourceType<TResourceType>(this JsonApiConfiguration jsonApiConfiguration,
            Action<ResourceTypeConfiguration<TResourceType>> configurationAction = null) where TResourceType : class
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
