using Autofac;
using JSONAPI.ActionFilters;
using JSONAPI.EntityFramework;
using JSONAPI.EntityFramework.ActionFilters;
using JSONAPI.EntityFramework.Http;
using JSONAPI.QueryableTransformers;

namespace JSONAPI.Autofac.EntityFramework
{
    public class JsonApiAutofacEntityFrameworkModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AsynchronousEnumerationTransformer>().As<IQueryableEnumerationTransformer>();
            builder.RegisterGeneric(typeof(EntityFrameworkDocumentMaterializer<>))
                .AsImplementedInterfaces();
            builder.RegisterType<EntityFrameworkResourceObjectMaterializer>()
                .As<IEntityFrameworkResourceObjectMaterializer>();
        }
    }
}
