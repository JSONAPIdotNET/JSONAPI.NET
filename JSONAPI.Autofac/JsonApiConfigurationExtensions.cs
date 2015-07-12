using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using JSONAPI.Configuration;

namespace JSONAPI.Autofac
{
    public static class JsonApiConfigurationExtensions
    {
        public static void SetupHttpConfigurationUsingAutofac(this IJsonApiConfiguration configuration,
            HttpConfiguration httpConfiguration, ILifetimeScope parentLifetimeScope)
        {
            var configurator = new JsonApiHttpAutofacConfigurator(parentLifetimeScope);
            configurator.Apply(httpConfiguration, configuration);
        }
    }
}
