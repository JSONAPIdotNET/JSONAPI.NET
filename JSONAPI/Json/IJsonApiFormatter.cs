using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// Interface responsible for serializing and deserializing JSON API document components
    /// </summary>
    /// <typeparam name="T">The type of component this service can format</typeparam>
    public interface IJsonApiFormatter<T>
    {
        /// <summary>
        /// Serializes the given component
        /// </summary>
        /// <param name="component">The component to serialize</param>
        /// <param name="writer">The JSON writer to serialize with</param>
        /// <returns>A task that resolves when serialization is complete.</returns>
        Task Serialize(T component, JsonWriter writer);

        /// <summary>
        /// Deserializes the given component
        /// </summary>
        /// <param name="reader">The JSON reader to deserialize with</param>
        /// <param name="currentPath">A JSON pointer pointing to the current thing being deserialized</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>A task that resolves with the deserialized object</returns>
        Task<T> Deserialize(JsonReader reader, string currentPath);
    }
}