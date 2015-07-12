using System;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using JSONAPI.Configuration;

namespace JSONAPI.Autofac
{
    public class JsonApiHttpAutofacConfigurator
    {
        private readonly ILifetimeScope _lifetimeScope;
        private Action<ILifetimeScope> _appLifetimeScopeBegunAction;

        public JsonApiHttpAutofacConfigurator(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public void OnApplicationLifetimeScopeBegun(Action<ILifetimeScope> appLifetimeScopeBegunAction)
        {
            _appLifetimeScopeBegunAction = appLifetimeScopeBegunAction;
        }

        public void Apply(HttpConfiguration httpConfiguration, IJsonApiConfiguration jsonApiConfiguration)
        {
            var applicationLifetimeScope = _lifetimeScope.BeginLifetimeScope(containerBuilder =>
            {
                var module = new JsonApiAutofacModule(jsonApiConfiguration);
                containerBuilder.RegisterModule(module);
            });

            if (_appLifetimeScopeBegunAction != null)
                _appLifetimeScopeBegunAction(applicationLifetimeScope);

            var jsonApiHttpConfiguration = applicationLifetimeScope.Resolve<JsonApiHttpConfiguration>();
            jsonApiHttpConfiguration.Apply(httpConfiguration);
            httpConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(applicationLifetimeScope);
        }
    }
}
