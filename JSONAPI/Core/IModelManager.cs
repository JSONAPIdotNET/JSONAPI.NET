using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.Core
{
    public interface IModelManager
    {
        IPluralizationService PluralizationService { get; }

        /// <summary>
        /// Returns the property that is treated as the unique identifier in a given class.
        /// This is used most importantly by JsonApiFormatter to determine what value to
        /// write when serializing a "Many" relationship as an array of Ids. It is also
        /// used to make dummy related objects (with only the Id property set) when
        /// deserializing a JSON payload that specifies a related object only by Id.
        /// 
        /// Rules for determining this may vary by implementation.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The property determined to represent the Id.</returns>
        PropertyInfo GetIdProperty(Type type);

        /// <summary>
        /// Returns the name that will be used to represent this type in json-api documents. 
        /// The `type` property of resource objects of this type will have this value.
        /// </summary>
        /// <param name="type">The serializable Type</param>
        /// <returns>The string denoting the given type in JSON documents.</returns>
        string GetResourceTypeNameForType(Type type);

        /// <summary>
        /// Gets the registered Type corresponding to a json-api resource type name. Inverse
        /// of <see cref="GetResourceTypeNameForType" />
        /// </summary>
        /// <param name="resourceTypeName"></param>
        /// <returns>The type that has been registered for this resource type name.</returns>
        Type GetTypeByResourceTypeName(string resourceTypeName);

        /// <summary>
        /// Determines whether a given type has been registered.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>Whether the type is registered</returns>
        bool TypeIsRegistered(Type type);

        /// <summary>
        /// Returns the property corresponding to a given JSON Key. Inverse of GetJsonKeyForProperty.
        /// </summary>
        /// <param name="type">The Type to find the property on</param>
        /// <param name="jsonKey">The JSON key representing a property</param>
        /// <returns></returns>
        ModelProperty GetPropertyForJsonKey(Type type, string jsonKey);

        /// <summary>
        /// Analogue to System.Type.GetProperties(), but made available so that any caching done
        /// by an IModelManager can be leveraged to return the results faster.
        /// </summary>
        /// <param name="type">The type to get properties from</param>
        /// <returns>All properties recognized by the IModelManager.</returns>
        ModelProperty[] GetProperties(Type type);

        /// <summary>
        /// Determines whether or not the given type will be treated as a "Many" relationship. 
        /// </summary>
        /// <param name="type">The serializable Type</param>
        /// <returns>True for Array and IEnumerable&lt;T&gt; types, false otherwise.</returns>
        bool IsSerializedAsMany(Type type);

        /// <summary>
        /// Analogue for System.Type.GetElementType, but works for arrays or IEnumerable&lt;T&gt;,
        /// and provides a capture point to cache potentially expensive reflection operations that
        /// have to occur repeatedly in JsonApiFormatter.
        /// </summary>
        /// <param name="manyType">A type which must be either an Array type or implement IEnumerable&lt;T&gt;.</param>
        /// <returns>The element type of an Array, or the first generic parameter of an IEnumerable&lt;T&gt;.</returns>
        Type GetElementType(Type manyType);

    }
}
