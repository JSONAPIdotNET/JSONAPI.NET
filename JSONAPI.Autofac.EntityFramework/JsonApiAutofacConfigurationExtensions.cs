using Autofac.Core;

namespace JSONAPI.Autofac.EntityFramework
{
    public static class JsonApiAutofacConfigurationExtensions
    {
        public static IModule GetEntityFrameworkAutofacModule(this JsonApiAutofacConfiguration jsonApiAutofacConfiguration)
        {
            return new JsonApiAutofacEntityFrameworkModule();
        }
    }
}
