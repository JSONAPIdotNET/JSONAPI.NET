using JSONAPI.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.Core
{
    internal class ModelManager
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

        private Lazy<Dictionary<Type, string>> _jsonKeysForType
            = new Lazy<Dictionary<Type, string>>(
                () => new Dictionary<Type, string>()
            );

        #endregion

        #region Id property determination

        public PropertyInfo GetIdProperty(Type type)
        {
            PropertyInfo idprop = null;

            var idPropCache = _idProperties.Value;

            lock (idPropCache)
            {
                if (idPropCache.TryGetValue(type, out idprop)) return idprop;

                //TODO: Enable attribute-based determination

                idprop = type.GetProperty("Id");

                if (idprop == null)
                    throw new InvalidOperationException(String.Format("Unable to determine Id property for type {0}", type));

                idPropCache.Add(type, idprop);
            }

            return idprop;
        }

        #endregion

        #region Property Maps

        public Dictionary<string, PropertyInfo> GetPropertyMap(Type type)
        {
            Dictionary<string, PropertyInfo> propMap = null;

            var propMapCache = _propertyMaps.Value;

            lock (propMapCache)
            {
                if (propMapCache.TryGetValue(type, out propMap)) return propMap;

                propMap = new Dictionary<string, PropertyInfo>();
                PropertyInfo[] props = type.GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    propMap[JsonApiFormatter.FormatPropertyName(prop.Name)] = prop;
                }

                propMapCache.Add(type, propMap);
            }

            return propMap;
        }

        #endregion

        //TODO: This has been "moved" here so we can cache the results and improve performance...but
        // it raises the question of whether the various methods called within here should belong
        // to JsonApiFormatter at all...should they move here also? Should the IPluralizationService
        // instance belong to ModelManager instead?
        internal string GetJsonKeyForType(Type type, IPluralizationService pluralizationService)
        {
            string key = null;

            var keyCache = _jsonKeysForType.Value;

            lock (keyCache)
            {
                if (keyCache.TryGetValue(type, out key)) return key;

                if (JsonApiFormatter.IsMany(type))
                    type = JsonApiFormatter.GetSingleType(type);

                var attrs = type.CustomAttributes.Where(x => x.AttributeType == typeof(Newtonsoft.Json.JsonObjectAttribute)).ToList();

                string title = type.Name;
                if (attrs.Any())
                {
                    var titles = attrs.First().NamedArguments.Where(arg => arg.MemberName == "Title")
                        .Select(arg => arg.TypedValue.Value.ToString()).ToList();
                    if (titles.Any()) title = titles.First();
                }

                key = JsonApiFormatter.FormatPropertyName(pluralizationService.Pluralize(title));

                keyCache.Add(type, key);
            }

            return key;
        }
    }
}
