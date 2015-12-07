using JSONAPI.Documents;

namespace JSONAPI.Core
{
    /// <summary>
    /// Populates property values on an ephemeral resource from a relationship object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEphemeralRelatedResourceReader<T>
    {
        /// <summary>
        /// Sets the property on the ephemeral resource that corresponds to the given property
        /// </summary>
        /// <param name="ephemeralResource"></param>
        /// <param name="jsonKey"></param>
        /// <param name="relationshipObject"></param>
        void SetProperty(T ephemeralResource, string jsonKey, IRelationshipObject relationshipObject);
    }
}