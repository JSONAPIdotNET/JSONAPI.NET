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
        /// Returns the key that will be used to represent a collection of objects of a
        /// given type, for example in the top-level of a JSON API document or within
        /// the "linked" objects section of a payload.
        /// </summary>
        /// <param name="type">The serializable Type</param>
        /// <returns>The string denoting the given type in JSON documents.</returns>
        string GetJsonKeyForType(Type type);

        /// <summary>
        /// Returns the key that will be used to represent the given property in serialized
        /// JSON. Inverse of GetPropertyForJsonKey.
        /// </summary>
        /// <param name="propInfo">The serializable property</param>
        /// <returns>The string denoting the given property within a JSON document.</returns>
        string GetJsonKeyForProperty(PropertyInfo propInfo); //TODO: Do we need to have a type parameter here, in case the property is inherited?

        /// <summary>
        /// Returns the property corresponding to a given JSON Key. Inverse of GetJsonKeyForType.
        /// </summary>
        /// <param name="type">The Type to find the property on</param>
        /// <param name="jsonKey">The JSON key representing a property</param>
        /// <returns></returns>
        PropertyInfo GetPropertyForJsonKey(Type type, string jsonKey);

        /// <summary>
        /// Analogue to System.Type.GetProperties(), but made available so that any caching done
        /// by an IModelManager can be leveraged to return the results faster.
        /// </summary>
        /// <param name="type">The type to get properties from</param>
        /// <returns>All properties recognized by the IModelManager.</returns>
        //TODO: This needs to include JsonIgnore'd properties, so that they can be found and explicitly included at runtime...confusing? Add another method that excludes these?
        PropertyInfo[] GetProperties(Type type);

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
