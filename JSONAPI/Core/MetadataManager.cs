using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.Core
{
    public sealed class MetadataManager : IMetadataManager
    {
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

        private readonly ConditionalWeakTable<object, Dictionary<string, object>> cwt
            = new ConditionalWeakTable<object, Dictionary<string, object>>();

        /*
        internal void SetDeserializationMetadata(object deserialized, Dictionary<string, object> meta)
        {
            cwt.Add(deserialized, meta);
        }
         */

        internal void SetMetaForProperty(object deserialized, PropertyInfo prop, object value)
        {
            Dictionary<string, object> meta;
            if (!cwt.TryGetValue(deserialized, out meta))
            {
                meta = new Dictionary<string, object>();
                cwt.Add(deserialized, meta);
            }
            if (!meta.ContainsKey(prop.Name)) // Temporary fix for non-standard Id reprecussions...this internal implementation will change soon anyway.
                meta.Add(prop.Name, value);
            
        }


        internal Dictionary<String, object> DeserializationMetadata(object deserialized)
        {
            Dictionary<string, object> retval;
            if (cwt.TryGetValue(deserialized, out retval))
            {
                return retval;
            }
            else
            {
                //TODO: Throw an exception here? If you asked for metadata for an object and it's not found, something has probably gone pretty badly wrong!
                return null;
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PropertyWasPresent(object deserialized, PropertyInfo prop)
        {
            object throwaway;
            return this.DeserializationMetadata(deserialized).TryGetValue(prop.Name, out throwaway);
        }
    }
}
