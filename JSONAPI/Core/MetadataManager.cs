using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.Core
{
    public sealed class MetadataManager
    {
        private class PropertyMetadata
        {
            public bool PresentInJson { get; set; } // only meaningful for incoming/deserialized models!
            public Lazy<ISet<System.Attribute>> AttributeOverrides
                = new Lazy<ISet<System.Attribute>>(
                    () => new HashSet<System.Attribute>()
                );
        }

        private class ModelMetadata
        {
            public Lazy<IDictionary<PropertyInfo, PropertyMetadata>> PropertyMetadata
                = new Lazy<IDictionary<PropertyInfo, PropertyMetadata>>(
                    () => new Dictionary<PropertyInfo, PropertyMetadata>()
                );
        }

        #region Singleton pattern

        private static readonly MetadataManager instance = new MetadataManager();

        private MetadataManager() { }

        public static MetadataManager Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        private readonly ConditionalWeakTable<object, ModelMetadata> cwt
            = new ConditionalWeakTable<object, ModelMetadata>();

        /*
        internal void SetDeserializationMetadata(object deserialized, Dictionary<string, object> meta)
        {
            cwt.Add(deserialized, meta);
        }
         */

        private ModelMetadata GetMetadataForModel(object model)
        {
            ModelMetadata meta;
            lock(cwt)
            {
                if (!cwt.TryGetValue(model, out meta))
                {
                    meta = new ModelMetadata();
                    cwt.Add(model, meta);
                }
            }
            return meta;
        }

        private PropertyMetadata GetMetadataForProperty(object model, PropertyInfo prop)
        {
            ModelMetadata mmeta = GetMetadataForModel(model);
            IDictionary<PropertyInfo, PropertyMetadata> pmetadict = mmeta.PropertyMetadata.Value;
            PropertyMetadata pmeta;
            lock (pmetadict)
            {
                if (!pmetadict.TryGetValue(prop, out pmeta))
                {
                    pmeta = new PropertyMetadata();
                    pmetadict.Add(prop, pmeta);
                }
            }
            return pmeta;
        }

        internal void SetPropertyWasPresent(object deserialized, PropertyInfo prop, bool value)
        {
            PropertyMetadata pmeta = GetMetadataForProperty(deserialized, prop);
            pmeta.PresentInJson = value;
        }

        /// <summary>
        /// Find whether or not a given property was
        /// posted in the original JSON--i.e. to determine whether an update operation should be
        /// performed, and/or if a default value should be used.
        /// </summary>
        /// <param name="deserialized">The object deserialized by JsonApiFormatter</param>
        /// <param name="prop">The property to check</param>
        /// <returns>Whether or not the property was found in the original JSON and set by the deserializer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PropertyWasPresent(object deserialized, PropertyInfo prop)
        {
            return this.GetMetadataForProperty(deserialized, prop).PresentInJson;
        }

        /// <summary>
        /// Set different serialization attributes at runtime than those that were declared on
        /// a property at compile time. E.g., if you declared a relationship property with
        /// [SerializeAs(SerializeAsOptions.Link)] but you want to change that to
        /// SerializeAsOptions.Ids when you are transmitting only one object, you can do:
        /// 
        ///     MetadataManager.SetPropertyAttributeOverrides(
        ///         theModelInstance, theProperty, 
        ///         new SerializeAsAttribute(SerializeAsOptions.Ids)
        ///     );
        /// 
        /// Further, if you want to also include the related objects in the serialized document:
        /// 
        ///     MetadataManager.SetPropertyAttributeOverrides(
        ///         theModelInstance, theProperty, 
        ///         new SerializeAs(SerializeAsOptions.Ids),
        ///         new IncludeInPayload(true)
        ///     );
        /// 
        /// Calling this function resets all overrides, so calling it twice will result in only
        /// the second set of overrides being applied. At present, the order of the attributes
        /// is not meaningful.
        /// </summary>
        /// <param name="model">The model object that is to be serialized, for which you want to change serialization behavior.</param>
        /// <param name="prop">The property for which to override attributes.</param>
        /// <param name="attrs">One or more attribute instances that will override the declared behavior.</param>
        public void SetPropertyAttributeOverrides(object model, PropertyInfo prop, params System.Attribute[] attrs)
        {
            var aoverrides = this.GetMetadataForProperty(model, prop).AttributeOverrides.Value;
            lock (aoverrides)
            {
                aoverrides.Clear();
                aoverrides.UnionWith(attrs);
            }
        }

        internal IEnumerable<System.Attribute> GetPropertyAttributeOverrides(object model, PropertyInfo prop)
        {
            return this.GetMetadataForProperty(model, prop).AttributeOverrides.Value;
        }

    }
}
