using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.EntityFramework
{
    public partial class EntityFrameworkMaterializer
    {
        public T GetDetachedOriginal<T>(T entity, bool fixupRelationships = false)
            where T : class
        {
            DbPropertyValues originalValues = DbContext.Entry<T>(entity).OriginalValues;
            T orig = (T)originalValues.ToObject();
            if (fixupRelationships)
            {
                throw new NotImplementedException();
            }
            return orig;
        }

        public IEnumerable<T2> GetAssociationChanges<T1,T2>(T1 parent, string propertyName, EntityState findState)
        {
            ObjectContext ocontext = ((IObjectContextAdapter)DbContext).ObjectContext;
            MetadataWorkspace metadataWorkspace = ocontext.MetadataWorkspace;

            // Find the AssociationType that matches the property traits given as input            
            AssociationType atype =
                metadataWorkspace.GetItems<AssociationType>(DataSpace.CSpace)
                .Where(a => a.AssociationEndMembers.Any(
                    ae => ae.MetadataProperties.Any(mp => mp.Name == "ClrPropertyInfo" // Magic string!!!
                        && ((PropertyInfo)mp.Value).Name == propertyName
                        && typeof(T1).IsAssignableFrom(((PropertyInfo)mp.Value).DeclaringType)
                        )
                        )
                        ).First();

            // Find added or deleted DbDataRecords from the above discovered type
            ocontext.DetectChanges();
            IEnumerable<DbDataRecord> dbDataRecords = ocontext.ObjectStateManager
                .GetObjectStateEntries(findState)
                .Where(e => e.IsRelationship)
                // Oddly, this works, while doing the same thing below requires comparing .Name...?
                .Where(e => e.EntitySet.ElementType == atype)
                .Select(e => findState == EntityState.Deleted ? e.OriginalValues : e.CurrentValues);

            // Get the actual entities using the EntityKeys in the DbDataRecord
            IList<T2> relationChanges = new List<T2>();
            foreach (System.Data.Common.DbDataRecord ddr in dbDataRecords)
            {
                EntityKey ek = (EntityKey)ddr[0];
                // Comparing .ElementType to atype doesn't work, see above...?
                if (!(ek.GetEntitySet(metadataWorkspace).ElementType.Name == atype.Name))
                {
                    ek = (EntityKey)ddr[1];
                }
                relationChanges.Add((T2)ocontext.GetObjectByKey(ek));
            }
            return relationChanges;
        }
    }
}
