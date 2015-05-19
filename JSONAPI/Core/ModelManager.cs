using System.Collections.ObjectModel;
using JSONAPI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JSONAPI.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JSONAPI.Core
{
    public class ModelManager : IModelManager
    {
        public ModelManager(IPluralizationService pluralizationService)
        {
            _pluralizationService = pluralizationService;
            RegistrationsByName = new Dictionary<string, TypeRegistration>();
            RegistrationsByType = new Dictionary<Type, TypeRegistration>();
        }

        protected IPluralizationService _pluralizationService = null;
        public IPluralizationService PluralizationService
        {
            get
            {
                return _pluralizationService;
            }
        }

        #region Cache storage

        /// <summary>
        /// Represents a type's registration with a model manager
        /// </summary>
        protected sealed class TypeRegistration
        {
            internal TypeRegistration() { }

            /// <summary>
            /// The type that has been registered
            /// </summary>
            public Type Type { get; internal set; }

            /// <summary>
            /// The serialized format of the type's name
            /// </summary>
            public string ResourceTypeName { get; internal set; }

            /// <summary>
            /// The property to be used as this type's ID.
            /// </summary>
            public PropertyInfo IdProperty { get; internal set; }

            /// <summary>
            /// A resource's properties, keyed by name.
            /// </summary>
            public IReadOnlyDictionary<string, ModelProperty> Properties { get; internal set; }
        }

        protected readonly IDictionary<string, TypeRegistration> RegistrationsByName;
        protected readonly IDictionary<Type, TypeRegistration> RegistrationsByType; 

        protected Lazy<Dictionary<Type, bool>> _isSerializedAsMany
            = new Lazy<Dictionary<Type, bool>>(
                () => new Dictionary<Type, bool>()
            );

        protected Lazy<Dictionary<Type, Type>> _getElementType
            = new Lazy<Dictionary<Type, Type>>(
                () => new Dictionary<Type, Type>()
            );

        #endregion

        public PropertyInfo GetIdProperty(Type type)
        {
            return GetRegistrationByType(type).IdProperty;
        }

        public ModelProperty[] GetProperties(Type type)
        {
            var typeRegistration = GetRegistrationByType(type);
            return typeRegistration.Properties.Values.ToArray();
        }

        public ModelProperty GetPropertyForJsonKey(Type type, string jsonKey)
        {
            var typeRegistration = GetRegistrationByType(type);
            ModelProperty property;
            return (typeRegistration.Properties.TryGetValue(jsonKey, out property)) ? property : null;
        }

        public string GetResourceTypeNameForType(Type type)
        {
            return GetRegistrationByType(type).ResourceTypeName;
        }

        public Type GetTypeByResourceTypeName(string resourceTypeName)
        {
            lock (RegistrationsByName)
            {
                TypeRegistration typeRegistration;
                if (RegistrationsByName.TryGetValue(resourceTypeName, out typeRegistration))
                    return typeRegistration.Type;

                throw new InvalidOperationException(String.Format("The resource type name `{0}` was not registered.",
                    resourceTypeName));
            }
        }

        public bool TypeIsRegistered(Type type)
        {
            var registration = FindRegistrationForType(type);
            return registration != null;
        }

        private TypeRegistration FindRegistrationForType(Type type)
        {
            lock (RegistrationsByType)
            {
                if (IsSerializedAsMany(type))
                    type = GetElementType(type);

                var currentType = type;
                while (currentType != null && currentType != typeof(Object))
                {
                    TypeRegistration registration;
                    if (RegistrationsByType.TryGetValue(currentType, out registration))
                        return registration;

                    // This particular type wasn't registered, but maybe the base type was.
                    currentType = currentType.BaseType;
                }
            }

            return null;
        }

        private TypeRegistration GetRegistrationByType(Type type)
        {
            var registration = FindRegistrationForType(type);
            if (registration != null) return registration;

            throw new InvalidOperationException(String.Format("The type `{0}` was not registered.", type.FullName));
        }

        /// <summary>
        /// Registers a type with this ModelManager.
        /// </summary>
        /// <param name="type">The type to register.</param>
        public ModelManager RegisterResourceType(Type type)
        {
            var resourceTypeName = CalculateResourceTypeNameForType(type);
            return RegisterResourceType(type, resourceTypeName);
        }

        /// <summary>
        /// Registeres a type with this ModelManager, using a default resource type name.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="resourceTypeName">The resource type name to use</param>
        public ModelManager RegisterResourceType(Type type, string resourceTypeName)
        {
            lock (RegistrationsByType)
            {
                lock (RegistrationsByName)
                {
                    if (RegistrationsByType.ContainsKey(type))
                        throw new InvalidOperationException(String.Format("The type `{0}` has already been registered.",
                            type.FullName));

                    if (RegistrationsByName.ContainsKey(resourceTypeName))
                        throw new InvalidOperationException(
                            String.Format("The resource type name `{0}` has already been registered.", resourceTypeName));

                    var registration = new TypeRegistration
                    {
                        Type = type,
                        ResourceTypeName = resourceTypeName
                    };

                    var propertyMap = new Dictionary<string, ModelProperty>();

                    var idProperty = CalculateIdProperty(type);
                    if (idProperty == null)
                        throw new InvalidOperationException(String.Format(
                            "Unable to determine Id property for type `{0}`.", resourceTypeName));

                    registration.IdProperty = idProperty;

                    var props = type.GetProperties();
                    foreach (var prop in props)
                    {
                        var jsonKey = prop == registration.IdProperty
                            ? "id"
                            : CalculateJsonKeyForProperty(prop);
                        if (propertyMap.ContainsKey(jsonKey))
                            throw new InvalidOperationException(
                                String.Format("The type `{0}` already contains a property keyed at `{1}`.",
                                    resourceTypeName, jsonKey));
                        var property = CreateModelProperty(prop, jsonKey);
                        propertyMap[jsonKey] = property;
                    }

                    registration.Properties = new ReadOnlyDictionary<string, ModelProperty>(propertyMap);


                    RegistrationsByType.Add(type, registration);
                    RegistrationsByName.Add(resourceTypeName, registration);
                }
            }

            return this;
        }

        /// <summary>
        /// Creates a cacheable model property representation from a PropertyInfo
        /// </summary>
        /// <param name="prop">The property</param>
        /// <param name="jsonKey">The key that this model property will be serialized as</param>
        /// <returns>A model property represenation</returns>
        protected virtual ModelProperty CreateModelProperty(PropertyInfo prop, string jsonKey)
        {
            var type = prop.PropertyType;
            var ignoreByDefault =
                prop.CustomAttributes.Any(c => c.AttributeType == typeof(JsonIgnoreAttribute));

            if (prop.PropertyType.CanWriteAsJsonApiAttribute())
                return new FieldModelProperty(prop, jsonKey, ignoreByDefault);

            var isToMany =
                type.IsArray ||
                (type.GetInterfaces().Contains(typeof(System.Collections.IEnumerable)) && type.IsGenericType);

            Type relatedType;
            if (isToMany)
            {
                relatedType = type.IsGenericType ? type.GetGenericArguments()[0] : type.GetElementType();
            }
            else
            {
                relatedType = type;
            }
            return new RelationshipModelProperty(prop, jsonKey, ignoreByDefault, relatedType, isToMany);
        }

        /// <summary>
        /// Determines the resource type name for a given type.
        /// </summary>
        /// <param name="type">The type to calculate the resouce type name for</param>
        /// <returns>The type's resource type name</returns>
        protected virtual string CalculateResourceTypeNameForType(Type type)
        {
            var attrs = type.CustomAttributes.Where(x => x.AttributeType == typeof(Newtonsoft.Json.JsonObjectAttribute)).ToList();

            string title = type.Name;
            if (attrs.Any())
            {
                var titles = attrs.First().NamedArguments.Where(arg => arg.MemberName == "Title")
                    .Select(arg => arg.TypedValue.Value.ToString()).ToList();
                if (titles.Any()) title = titles.First();
            }

            return FormatPropertyName(PluralizationService.Pluralize(title)).Dasherize();
        }

        /// <summary>
        /// Determines the key that a property will be serialized as.
        /// </summary>
        /// <param name="propInfo">The property</param>
        /// <returns>The key to serialize the given property as</returns>
        protected internal virtual string CalculateJsonKeyForProperty(PropertyInfo propInfo)
        {
            var jsonPropertyAttribute = (JsonPropertyAttribute)propInfo.GetCustomAttributes(typeof (JsonPropertyAttribute)).FirstOrDefault();
            return jsonPropertyAttribute != null ? jsonPropertyAttribute.PropertyName : FormatPropertyName(propInfo.Name);
        }

        private static string FormatPropertyName(string propertyName)
        {
            string result = propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1);
            return result;
        }

        public bool IsSerializedAsMany(Type type)
        {
            bool isMany;

            var isManyCache = _isSerializedAsMany.Value;

            lock (isManyCache)
            {
                if (isManyCache.TryGetValue(type, out isMany)) return isMany;

                isMany =
                    type.IsArray ||
                    (type.GetInterfaces().Contains(typeof(System.Collections.IEnumerable)) && type.IsGenericType);

                isManyCache.Add(type, isMany);
            }

            return isMany;
        }

        public Type GetElementType(Type manyType)
        {
            Type etype = null;

            var etypeCache = _getElementType.Value;

            lock (etypeCache)
            {
                if (etypeCache.TryGetValue(manyType, out etype)) return etype;

                if (manyType.IsGenericType)
                    etype = manyType.GetGenericArguments()[0];
                else
                    etype = manyType.GetElementType();

                etypeCache.Add(manyType, etype);
            }

            return etype;
        }

        /// <summary>
        /// Calculates the ID property for a given resource type.
        /// </summary>
        /// <param name="type">The type to use to calculate the ID for</param>
        /// <returns>The ID property to use for this type</returns>
        protected virtual PropertyInfo CalculateIdProperty(Type type)
        {
            return
                type
                    .GetProperties()
                    .FirstOrDefault(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(UseAsIdAttribute)))
                ?? type.GetProperty("Id");
        }
    }
}
