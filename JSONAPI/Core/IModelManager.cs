using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.Core
{
    /// <summary>
    /// <para>
    /// The <c>IModelManager</c> interface defines a set of methods used to introspect
    /// model objects for serialization and deserialization purposes. For most users,
    /// the provided <see cref="T:JSONAPI.Core.ModelManager"/> will be sufficient,
    /// but you can also provide your own implementation if needed.
    /// </para>
    /// <para>
    /// One of the reasons for the existence of <c>IModelManager</c> (and the
    /// provided implementation <see cref="T:JSONAPI.Core.ModelManager"/>)
    /// is to be able to capture and cache the results of operations that can be very
    /// expensive due to the use of reflection. Consider caching the results
    /// of most of the methods if you use reflection extensively in your own
    /// implementation of <c>IModelManager</c>, because these methods will be
    /// called by <see cref="T:JSONAPI.Json.JsonApiFormatter"/> many many times.
    /// </para>
    /// <para>
    /// To use an IModelManager, pass it to <see cref="M:JSONAPI.Json.JsonApiFormatter.#ctor(JSONAPI.Core.IModelManager)"/>.
    /// </para>
    /// <seealso cref="T:JSONAPI.Core.ModelManager"/>
    /// </summary>
    public interface IModelManager
    {
        /// <summary>
        /// Provides (get-only) access to the <see cref="T:JSONAPI.Core.IPluralizationService"/> instance attached to this <c>IModelManager</c>.
        /// Any <c>IModelManager</c> must expose an <c>IPluralizationService</c> through this property for use by the <see cref="T:JSONAPI.Json.JsonApiFormatter"/>.
        /// Usually your <c>IModelManager</c> would accept an instance of <c>IPluralizationService</c> as a contructor parameter, but it might also
        /// always create one of a particular class. It would also be possible for an <c>IModelManager</c> implementation to also implement
        /// <c>IPluralizationService</c> and simply return itself for this property.
        /// </summary>
        IPluralizationService PluralizationService { get; }

        /// <summary>
        /// <para>
        /// Returns the property that is treated as the unique identifier in a given class.
        /// This is used most importantly by <see cref="T:JSONAPI.Json.JsonApiFormatter"/> to determine what value to
        /// write when serializing a "Many" relationship as an array of Ids. It is also
        /// used to make dummy related objects (with only the ID property set) when
        /// deserializing a JSON payload that specifies a related object only by ID.
        /// </para>
        /// <para>
        /// Rules for determining this may vary by implementation.
        /// </para>
        /// </summary>
        /// <param name="type">The model class</param>
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
        /// JSON. Inverse of <see cref="GetPropertyForJsonKey(Type,string)"/>.
        /// </summary>
        /// <param name="propInfo">The serializable property</param>
        /// <returns>The string denoting the given property within a JSON document.</returns>
        string GetJsonKeyForProperty(PropertyInfo propInfo); //TODO: Do we need to have a type parameter here, in case the property is inherited?

        /// <summary>
        /// Returns the property corresponding to a given JSON Key. Inverse of <see cref="GetJsonKeyForProperty(PropertyInfo)"/>.
        /// </summary>
        /// <param name="type">The Type to find the property on</param>
        /// <param name="jsonKey">The JSON key representing a property</param>
        /// <returns></returns>
        PropertyInfo GetPropertyForJsonKey(Type type, string jsonKey);

        /// <summary>
        /// Analogue to <see cref="Type.GetProperties()"/>, but made available so that any caching done
        /// by an <c>IModelManager</c> can be leveraged to return the results faster.
        /// </summary>
        /// <param name="type">The type to get properties from</param>
        /// <returns>All properties recognized by the <c>IModelManager</c>.</returns>
        //TODO: This needs to include JsonIgnore'd properties, so that they can be found and explicitly included at runtime...confusing? Add another method that excludes these?
        PropertyInfo[] GetProperties(Type type);

        /// <summary>
        /// Determines whether or not the given type will be treated as a "Many" relationship. 
        /// </summary>
        /// <param name="type">The serializable Type</param>
        /// <returns>True for Array and <see cref="IEnumerable{T}"/> types, false otherwise.</returns>
        bool IsSerializedAsMany(Type type);

        /// <summary>
        /// Analogue for <see cref="Type.GetElementType()"/>, but works for arrays or <see cref="IEnumerable{T}"/>,
        /// and provides a capture point to cache potentially expensive reflection operations that
        /// have to occur repeatedly in <see cref="JSONAPI.Json.JsonApiFormatter"/>.
        /// </summary>
        /// <param name="manyType">A type which must be either an Array type or implement <see cref="IEnumerable{T}"/>.</param>
        /// <returns>The element type of an Array, or the first generic parameter of an <see cref="IEnumerable{T}"/>.</returns>
        Type GetElementType(Type manyType);

    }
}
