using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using JSONAPI.Extensions;

namespace JSONAPI.Http
{
    /// <summary>
    /// Chooses a controller based on the pascal-case version of the default controller name
    /// </summary>
    public class PascalizedControllerSelector : DefaultHttpControllerSelector
    {
        /// <param name="configuration">The configuration to use</param>
        public PascalizedControllerSelector(HttpConfiguration configuration) : base(configuration)
        {
        }

        public override string GetControllerName(HttpRequestMessage request)
        {
            var baseControllerName = base.GetControllerName(request);
            return baseControllerName.Pascalize();
        }
    }
}