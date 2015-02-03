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
            public Lazy<IEnumerable<System.Attribute>> AttributeOverrides
                = new Lazy<IEnumerable<System.Attribute>>(
                    () => new List<System.Attribute>()
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

    }
}
