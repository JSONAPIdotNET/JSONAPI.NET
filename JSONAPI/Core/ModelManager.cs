using JSONAPI.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.Core
{
    class ModelManager
    {
        #region Singleton pattern

        private static readonly ModelManager instance = new ModelManager();

        private ModelManager() { }

        public static ModelManager Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region Cache storage

        private Lazy<Dictionary<Type, PropertyInfo>> _idProperties
            = new Lazy<Dictionary<Type,PropertyInfo>>(
                () => new Dictionary<Type, PropertyInfo>()
            );

        private Lazy<Dictionary<Type, Dictionary<string, PropertyInfo>>> _propertyMaps
            = new Lazy<Dictionary<Type, Dictionary<string, PropertyInfo>>>(
                () => new Dictionary<Type, Dictionary<string, PropertyInfo>>()
            );

        #endregion

        #region Id property determination

        public PropertyInfo GetIdProperty(Type type)
        {
            PropertyInfo idprop = null;

            var idPropCache = _idProperties.Value;

            if (idPropCache.TryGetValue(type, out idprop)) return idprop;
            
            //TODO: Enable attribute-based determination

            idprop = type.GetProperty("Id");

            if (idprop == null)
                throw new InvalidOperationException(String.Format("Unable to determine Id property for type {0}", type));

            _idProperties.Value.Add(type, idprop);

            return idprop;
        }

        #endregion

        #region Property Maps

        public Dictionary<string, PropertyInfo> GetPropertyMap(Type type)
        {
            Dictionary<string, PropertyInfo> propMap = null;

            var propMapCache = _propertyMaps.Value;

            if (propMapCache.TryGetValue(type, out propMap)) return propMap;

            propMap = new Dictionary<string, PropertyInfo>();
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                propMap[JsonApiFormatter.FormatPropertyName(prop.Name)] = prop;
            }

            propMapCache.Add(type, propMap);

            return propMap;
        }

        #endregion
    }
}
