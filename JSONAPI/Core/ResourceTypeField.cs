using System.Reflection;

namespace JSONAPI.Core
{
    /// <summary>
    /// Stores a resource type's field and its usage.
    /// </summary>
    public abstract class ResourceTypeField
    {
        internal ResourceTypeField(PropertyInfo property, string jsonKey)
        {
            JsonKey = jsonKey;
            Property = property;
        }

        /// <summary>
        /// The PropertyInfo backing this ModelProperty
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// The key that will be used to represent this property in JSON API documents
        /// </summary>
        public string JsonKey { get; private set; }
    }
}
