using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using JSONAPI.Core;

namespace JSONAPI.Autofac
{
    public static class HttpConfigurationExtensions
    {
        public static void UseJsonApiWithAutofac(this HttpConfiguration httpConfig, ILifetimeScope applicationLifetimeScope)
        {
            var jsonApiConfiguration = applicationLifetimeScope.Resolve<JsonApiHttpConfiguration>();
            jsonApiConfiguration.Apply(httpConfig);
            httpConfig.DependencyResolver = new AutofacWebApiDependencyResolver(applicationLifetimeScope);
        }
    }
}
