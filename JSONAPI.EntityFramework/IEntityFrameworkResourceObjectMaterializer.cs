using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Documents;
using JSONAPI.Json;

namespace JSONAPI.EntityFramework
{
    /// <summary>
    /// This class manages converting IResourceObject instances from a request into records managed
    /// by Entity Framework.
    /// </summary>
    public interface IEntityFrameworkResourceObjectMaterializer
    {
        /// <summary>
        /// Gets a record managed by Entity Framework that has merged in the data from
        /// the supplied resource object.
        /// </summary>
        /// <param name="resourceObject"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="DeserializationException"></exception>
        Task<object> MaterializeResourceObject(IResourceObject resourceObject, CancellationToken cancellationToken);
    }
}