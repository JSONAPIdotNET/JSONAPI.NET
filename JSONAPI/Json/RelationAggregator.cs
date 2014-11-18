using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.Json
{
    public class RelationAggregator
    {
        private Type itemType = null;
        private ISet<object> rootItems = null;
        internal Dictionary<Type, ISet<object>> Appendices;

        public RelationAggregator()
        {
            this.Appendices = new Dictionary<Type, ISet<object>>();
        }

        public void AddPrimary(Type type, IEnumerable<object> items)
        {
            if (itemType == null) itemType = type;
            if (rootItems == null)
                rootItems = new HashSet<object>();
            rootItems.UnionWith(items);
        }
        public void AddPrimary(Type type, object item)
        {
            if (itemType == null) itemType = type;
            if (rootItems == null)
                rootItems = new HashSet<object>();
            rootItems.Add(item);
        }

        public void Add(Type type, IEnumerable<object> items)
        {
            // Exclude items that are already included in the root!
            items = items.Except(this.rootItems);
            if (items.Count() <= 0) return;

            if (!this.Appendices.ContainsKey(type))
                //TODO: Can we make a strongly-typed collection here somehow, since we know the type?
                this.Appendices[type] = new HashSet<object>();

            this.Appendices[type].UnionWith(items);
            /* Assuming the above is faster than this...
            foreach (object item in items)
            {
                currentPayload.Appendices[prop.GetType()].Add(item);
            }
            */
        }

        public void Add(Type type, object item)
        {
            // Exclude items that are already included in the root!
            if (item.GetType() == itemType)
            {
                if (this.rootItems.Contains(item)) return;
            }

            if (!this.Appendices.ContainsKey(type))
                this.Appendices[type] = new HashSet<object>();

            this.Appendices[type].Add(item);
        }
    }
}
