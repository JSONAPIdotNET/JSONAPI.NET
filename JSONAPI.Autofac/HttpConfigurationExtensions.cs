using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using JSONAPI.Core;

namespace JSONAPI.Autofac
{
    public static class HttpConfigurationExtensions
    {
        public static void UseJsonApiWithAutofac(this HttpConfiguration httpConfig, IContainer applicationContainer)
        {
            var jsonApiConfiguration = applicationContainer.Resolve<JsonApiHttpConfiguration>();
            jsonApiConfiguration.Apply(httpConfig);
            httpConfig.DependencyResolver = new AutofacWebApiDependencyResolver(applicationContainer);
        }
    }
}
