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
        private Action<ContainerBuilder> _appLifetimeScopeCreating;
        private Action<ILifetimeScope> _appLifetimeScopeBegunAction;

        public JsonApiHttpAutofacConfigurator()
        {
        }

        public JsonApiHttpAutofacConfigurator(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public void OnApplicationLifetimeScopeCreating(Action<ContainerBuilder> appLifetimeScopeCreating)
        {
            _appLifetimeScopeCreating = appLifetimeScopeCreating;
        }

        public void OnApplicationLifetimeScopeBegun(Action<ILifetimeScope> appLifetimeScopeBegunAction)
        {
            _appLifetimeScopeBegunAction = appLifetimeScopeBegunAction;
        }

        public void Apply(HttpConfiguration httpConfiguration, IJsonApiConfiguration jsonApiConfiguration)
        {
            ILifetimeScope applicationLifetimeScope;
            if (_lifetimeScope == null)
            {
                var builder = new ContainerBuilder();
                ConfigureApplicationLifetimeScope(jsonApiConfiguration, builder);
                applicationLifetimeScope = builder.Build();
            }
            else
            {
                applicationLifetimeScope = _lifetimeScope.BeginLifetimeScope(containerBuilder =>
                {
                    ConfigureApplicationLifetimeScope(jsonApiConfiguration, containerBuilder);
                });
            }

            if (_appLifetimeScopeBegunAction != null)
                _appLifetimeScopeBegunAction(applicationLifetimeScope);

            var jsonApiHttpConfiguration = applicationLifetimeScope.Resolve<JsonApiHttpConfiguration>();
            jsonApiHttpConfiguration.Apply(httpConfiguration);
            httpConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(applicationLifetimeScope);
        }

        private void ConfigureApplicationLifetimeScope(IJsonApiConfiguration jsonApiConfiguration, ContainerBuilder containerBuilder)
        {
            var module = new JsonApiAutofacModule(jsonApiConfiguration);
            containerBuilder.RegisterModule(module);

            if (_appLifetimeScopeCreating != null)
                _appLifetimeScopeCreating(containerBuilder); 
        }
    }
}
