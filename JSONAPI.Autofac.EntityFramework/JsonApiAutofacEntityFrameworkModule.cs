using System;
using System.Linq;
using Autofac;
using JSONAPI.Core;
using JSONAPI.EntityFramework;
using JSONAPI.EntityFramework.ActionFilters;
using JSONAPI.EntityFramework.Http;
using JSONAPI.Http;
using JSONAPI.QueryableTransformers;

namespace JSONAPI.Autofac.EntityFramework
{
    public class JsonApiAutofacEntityFrameworkModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AsynchronousEnumerationTransformer>().As<IQueryableEnumerationTransformer>();
            builder.RegisterType<EntityFrameworkResourceObjectMaterializer>()
                .As<IEntityFrameworkResourceObjectMaterializer>();

            builder.RegisterGeneric(typeof (EntityFrameworkDocumentMaterializer<>));
            builder.Register((ctx, parameters) =>
            {
                var allParameters = parameters.ToArray();
                var typedParameters = allParameters.OfType<TypedParameter>().ToArray();
                var resourceTypeRegistrationParameter =
                    typedParameters.FirstOrDefault(tp => tp.Type == typeof(IResourceTypeRegistration));
                if (resourceTypeRegistrationParameter == null)
                    throw new Exception(
                        "An IResourceTypeRegistration parameter must be provided to resolve an instance of EntityFrameworkDocumentMaterializer.");

                var resourceTypeRegistration = resourceTypeRegistrationParameter.Value as IResourceTypeRegistration;
                if (resourceTypeRegistration == null)
                    throw new Exception(
                        "An IResourceTypeRegistration parameter was provided to resolve EntityFrameworkDocumentMaterializer, but its value was null.");

                var openGenericType = typeof (EntityFrameworkDocumentMaterializer<>);
                var materializerType = openGenericType.MakeGenericType(resourceTypeRegistration.Type);
                return ctx.Resolve(materializerType, allParameters);
            }).As<IDocumentMaterializer>();

            builder.RegisterGeneric(typeof(EntityFrameworkToManyRelatedResourceDocumentMaterializer<,>));
            builder.RegisterGeneric(typeof(EntityFrameworkToOneRelatedResourceDocumentMaterializer<,>));
            builder.Register((ctx, parameters) =>
            {
                var allParameters = parameters.ToArray();
                var typedParameters = allParameters.OfType<TypedParameter>().ToArray();
                var resourceTypeRegistrationParameter =
                    typedParameters.FirstOrDefault(tp => tp.Type == typeof(IResourceTypeRegistration));
                if (resourceTypeRegistrationParameter == null)
                    throw new Exception(
                        "An IResourceTypeRegistration parameter must be provided to resolve an instance of EntityFrameworkDocumentMaterializer.");

                var resourceTypeRegistration = resourceTypeRegistrationParameter.Value as IResourceTypeRegistration;
                if (resourceTypeRegistration == null)
                    throw new Exception(
                        "An IResourceTypeRegistration parameter was provided to resolve EntityFrameworkDocumentMaterializer, but its value was null.");

                var resourceTypeRelationshipParameter =
                    typedParameters.FirstOrDefault(tp => tp.Type == typeof(ResourceTypeRelationship));
                if (resourceTypeRelationshipParameter == null)
                    throw new Exception(
                        "A ResourceTypeRelationship parameter must be provided to resolve an instance of EntityFrameworkDocumentMaterializer.");

                var resourceTypeRelationship = resourceTypeRelationshipParameter.Value as ResourceTypeRelationship;
                if (resourceTypeRelationship == null)
                    throw new Exception(
                        "A ResourceTypeRelationship parameter was provided to resolve EntityFrameworkDocumentMaterializer, but its value was null.");

                var openGenericType = resourceTypeRelationship.IsToMany
                    ? typeof (EntityFrameworkToManyRelatedResourceDocumentMaterializer<,>)
                    : typeof (EntityFrameworkToOneRelatedResourceDocumentMaterializer<,>);
                var materializerType = openGenericType.MakeGenericType(resourceTypeRegistration.Type,
                    resourceTypeRelationship.RelatedType);
                return ctx.Resolve(materializerType, allParameters);
            }).As<IRelatedResourceDocumentMaterializer>();
        }
    }
}
