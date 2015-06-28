using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Default implementation of IPayload
    /// </summary>
    public class SingleResourcePayload : ISingleResourcePayload
    {
        public IResourceObject PrimaryData { get; private set; }

        public IResourceObject[] RelatedData { get; private set; }

        public IMetadata Metadata { get; private set; }

        /// <summary>
        /// Constructs a single resource payload
        /// </summary>
        /// <param name="primaryData"></param>
        /// <param name="relatedData"></param>
        /// <param name="metadata"></param>
        public SingleResourcePayload(IResourceObject primaryData, IResourceObject[] relatedData, IMetadata metadata)
        {
            PrimaryData = primaryData;
            RelatedData = relatedData;
            Metadata = metadata;
        }
    }
}
