using System.Reflection;

namespace JSONAPI.Core
{
    /// <summary>
    /// Manages request-specific metadata
    /// </summary>
    public interface IMetadataManager
    {
        /// <summary>
        /// Find whether or not a given property was
        /// posted in the original JSON--i.e. to determine whether an update operation should be
        /// performed, and/or if a default value should be used.
        /// </summary>
        /// <param name="deserialized">The object deserialized by JsonApiFormatter</param>
        /// <param name="prop">The property to check</param>
        /// <returns>Whether or not the property was found in the original JSON and set by the deserializer</returns>
        bool PropertyWasPresent(object deserialized, PropertyInfo prop);
    }
}
