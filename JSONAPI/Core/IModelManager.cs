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
        string GetJsonKeyForProperty(PropertyInfo propInfo); //TODO: Do we need to have a type parameter here, in case propInfo is inherited?
        PropertyInfo GetPropertyForJsonKey(Type type, string jsonKey);

        [Obsolete]
        IDictionary<string, PropertyInfo> GetPropertyMap(Type type);
    }
}
