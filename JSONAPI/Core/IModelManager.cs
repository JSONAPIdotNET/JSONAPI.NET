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

        PropertyInfo GetIdProperty(Type type);
        string GetJsonKeyForType(Type type);
        string GetJsonKeyForProperty(PropertyInfo propInfo); //TODO: Do we need to have a type parameter here, in case the property is inherited?
        PropertyInfo GetPropertyForJsonKey(Type type, string jsonKey);

        /// <summary>
        /// Analogue to System.Type.GetProperties(), but made available so that any caching done
        /// by an IModelManager can be leveraged to return the results faster.
        /// </summary>
        /// <param name="type">The type to get properties from</param>
        /// <returns>All properties recognized by the IModelManager.</returns>
        //TODO: This needs to include JsonIgnore'd properties, so that they can be found and explicitly included at runtime...confusing? Add another method that excludes these?
        PropertyInfo[] GetProperties(Type type);

        //[Obsolete]
        //IDictionary<string, PropertyInfo> GetPropertyMap(Type type);
    }
}
