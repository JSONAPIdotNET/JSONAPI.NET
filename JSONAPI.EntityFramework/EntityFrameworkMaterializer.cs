using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using JSONAPI.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Reflection;
using System.Collections;
using System.Data.Entity.Core;
using JSONAPI.Extensions;

namespace JSONAPI.EntityFramework
{
    /// <summary>
    /// IMaterializer implementation for use with Entity Framework
    /// </summary>
    public partial class EntityFrameworkMaterializer : IMaterializer
    {
        protected readonly IMetadataManager MetadataManager;

        /// <summary>
        /// The DbContext instance used to perform materializer operations
        /// </summary>
        public DbContext DbContext { get; private set; }

        /// <summary>
        /// Creates a new EntityFrameworkMaterializer.
        /// </summary>
        /// <param name="context">The DbContext instance used to perform materializer operations</param>
        public EntityFrameworkMaterializer(DbContext context, IMetadataManager metadataManager)
        {
            MetadataManager = metadataManager;
            DbContext = context;
        }

        #region IMaterializer contract methods

        /// <summary>
        /// Override here if you have mixed models and need to have child objects that are not supposed to be entities!
        /// </summary>
        /// <param name="type"></param>
        /// <param name="idValues"></param>
        /// <returns></returns>
        public async Task<T> GetByIdAsync<T>(params Object[] idValues) where T : class
        {
            //TODO: How to react if the type isn't in the context?

            // Input will probably usually be strings... make sure the right types are passed to .Find()...
            Object[] idv2 = new Object[idValues.Length];
            int i = 0;
            foreach (PropertyInfo prop in GetKeyProperties(typeof(T)))
            {
                try
                {
                    idv2[i] = ((IConvertible)idValues[i]).ToType(prop.PropertyType, null);
                }
                catch (Exception e)
                {
                    // Fallback...use what they gave us...
                    idv2[i] = idValues[i];
                }
                finally
                {
                    i++;
                }
            }
            return await DbContext.Set<T>().FindAsync(idv2);
        }

        public virtual async Task<T> MaterializeAsync<T>(T ephemeral) where T : class
        {
            var type = typeof (T);
            IEnumerable<string> keyNames = GetKeyNames(type);
            List<Object> idValues = new List<Object>();
            bool anyNull = false;
            foreach (string propName in keyNames)
            {
                PropertyInfo propInfo = type.GetProperty(propName);
                Object value = propInfo.GetValue(ephemeral);
                if (value == null)
                {
                    anyNull = true;
                    break;
                }
                idValues.Add(value);
            }
            T retval = null;
            if (!anyNull)
            {
                retval = await DbContext.Set<T>().FindAsync(idValues.ToArray());
            }
            if (retval == null)
            {
                // Didn't find it...create a new one!
                retval = (T)Activator.CreateInstance(type);

                DbContext.Set(type).Add(retval);
                if (!anyNull)
                {
                    // For a new object, if a key is specified, we want to merge the key, at least.
                    // For simplicity then, make the behavior equivalent to MergeMaterialize in this case.
                    await this.Merge(type, ephemeral, retval);
                }
            }
            return retval;
        }

        public async Task<T> MaterializeUpdateAsync<T>(T ephemeral) where T : class
        {
            var material = await MaterializeAsync(ephemeral);
            await Merge(typeof(T), ephemeral, material);
            return material;
        }

        #endregion

        private bool IsMany(Type type)
        {
            //TODO: Should we check for arrays also? (They aren't generics.)
            return (type.GetInterfaces().Contains(typeof(IEnumerable)) && type.IsGenericType);
        }

        public bool IsModel(Type objectType)
        {
            if (objectType.CanWriteAsJsonApiAttribute())
                return false;
            else return true;
        }

        private Type GetSingleType(Type type)
        {
            if (IsMany(type))
                return type.IsGenericType ? type.GenericTypeArguments[0] : type.GetElementType();
            else
                return type;
        }

        private static Lazy<MethodInfo> OpenGetKeyNamesFromGenericMethod =
            new Lazy<MethodInfo>(
                () =>
                    typeof (EntityFrameworkMaterializer).GetMethod("GetKeyNamesFromGeneric",
                        BindingFlags.NonPublic | BindingFlags.Static));

        protected internal virtual IEnumerable<string> GetKeyNames(Type type)
        {
            var openMethod = OpenGetKeyNamesFromGenericMethod.Value;
            var method = openMethod.MakeGenericMethod(type);
            try
            {
                return (IEnumerable<string>)method.Invoke(null, new object[] { DbContext });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private static IEnumerable<string> GetKeyNamesFromGeneric<T>(DbContext dbContext) where T : class
        {
            var objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;
            ObjectSet<T> objectSet;
            try
            {
                objectSet = objectContext.CreateObjectSet<T>();
            }
            catch (ArgumentException e)
            {
                var baseClass = typeof (T).BaseType;
                if (baseClass != null && baseClass != typeof (Object))
                {
                    var openMethod = OpenGetKeyNamesFromGenericMethod.Value;
                    var method = openMethod.MakeGenericMethod(baseClass);
                    return (IEnumerable<string>)method.Invoke(null, new object[] { dbContext });
                }

                throw new ArgumentException(
                    String.Format("The Type {0} was not found in the DbContext with Type {1}", typeof(T).Name, dbContext.GetType().Name),
                    e
                    );
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    String.Format("The Type {0} was not found in the DbContext with Type {1}", typeof(T).Name, dbContext.GetType().Name),
                    e
                    );
            }
            return objectSet.EntitySet.ElementType.KeyMembers.Select(k => k.Name).ToArray();
        }

        /// <summary>
        /// Convenience wrapper around GetKeyNames.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IEnumerable<PropertyInfo> GetKeyProperties(Type type)
        {
            IEnumerable<string> keyNames = GetKeyNames(type);
            List<PropertyInfo> retval = new List<PropertyInfo>();
            foreach (string propName in keyNames)
            {
                retval.Add(type.GetProperty(propName));
            }
            return retval;
        }

        /// <summary>
        /// Gets the name of the entity set property on the db context corresponding to the given type.
        /// </summary>
        /// <param name="type">Type type to get the entity set name for.</param>
        /// <returns>The name of the entity set property</returns>
        protected string GetEntitySetName(Type type)
        {
            ObjectContext objectContext = ((IObjectContextAdapter)DbContext).ObjectContext;
            try
            {
                var container = objectContext.MetadataWorkspace
                    .GetEntityContainer(objectContext.DefaultContainerName, System.Data.Entity.Core.Metadata.Edm.DataSpace.CSpace);
                // If this fails, type must not be in the context!!!
                string esname = (from meta in container.BaseEntitySets
                                 where meta.ElementType.Name == type.Name
                                 select meta.Name).FirstOrDefault();
                if (esname == null)
                {
                    return null;
                }
                return container.Name + "." + esname;
            }
            catch (Exception e)
            {
                // Type is not an entity type in the context. Return null.
                return null;
            }
        }

        /// <summary>
        /// Will return an entity key for any object. If it is not actually an entity (that is, the type
        /// is not found in the context as some DbSet), it will create a dummy EntityKey based on whatever
        /// GetKeyNames says is the primary key. Override GetKeyNames to make this work properly for your
        /// own model objects, see GetKeyNames for more information.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected EntityKey MaterializeEntityKey(Type type, object obj)
        {
            IEnumerable<PropertyInfo> keyProps = GetKeyProperties(type);

            // Build the key pairs...
            IList<KeyValuePair<string, object>> entityKeyValues =
                new List<KeyValuePair<string, object>>();
            foreach (PropertyInfo propertyInfo in keyProps)
            {
                entityKeyValues.Add(
                    new KeyValuePair<string, object>(
                        propertyInfo.Name,
                        propertyInfo.GetValue(obj, null)
                    )
                );
            }

            string esname = GetEntitySetName(type);
            // esname does not have to be real!
            if (esname == null) esname = type.Namespace + "." + type.Name;

            EntityKey key = new EntityKey(esname, entityKeyValues);

            return key;
        }

        protected async Task Merge (Type type, object ephemeral, object material)
        {
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                // Comply with the spec, if a key was not set, it should not be updated!
                if (!MetadataManager.PropertyWasPresent(ephemeral, prop)) continue;

                if (IsMany(prop.PropertyType))
                {
                    Type elementType = GetSingleType(prop.PropertyType);

                    var materialMany = (IEnumerable<Object>)prop.GetValue(material, null);
                    if (materialMany == null)
                    {
                        materialMany = prop.PropertyType.CreateEnumerableInstance();
                        prop.SetValue(material, materialMany);
                    }

                    var ephemeralMany = (IEnumerable<Object>)prop.GetValue(ephemeral, null);

                    var materialKeys = new HashSet<EntityKey>();
                    var ephemeralKeys = new HashSet<EntityKey>();
                    foreach (object child in materialMany)
                    {
                        //TODO: Faster to get the Entity Key from the object context? This seems like it may be just as efficient...
                        // see http://social.msdn.microsoft.com/Forums/en-US/479f2b2e-fdaa-4d9d-91da-772ba0ee8d55/ef40-how-to-get-entitykey-from-new-poco-?forum=adonetefx
                        materialKeys.Add(MaterializeEntityKey(elementType, child));
                    }
                    foreach (object child in ephemeralMany)
                    {
                        ephemeralKeys.Add(MaterializeEntityKey(elementType, child));
                    }

                    // We're having problems with how to generalize/cast/generic-ize this code, so for the time
                    // being we'll brute-force it in super-dynamic language style...
                    Type mmtype = materialMany.GetType();
                    MethodInfo mmadd = mmtype.GetMethod("Add");
                    MethodInfo mmremove = mmtype.GetMethod("Remove");

                    var openGenericGetByIdAsyncMethod = GetType().GetMethod("GetByIdAsync");
                    var closedGenericGetByIdAsyncMethod = openGenericGetByIdAsyncMethod.MakeGenericMethod(elementType);

                    // Add to hasMany
                    if (mmadd != null)
                        foreach (EntityKey key in ephemeralKeys.Except(materialKeys))
                        {
                            object[] idParams = key.EntityKeyValues.Select(ekv => ekv.Value).ToArray();
                            var obj = (object)await (dynamic)closedGenericGetByIdAsyncMethod.Invoke(this, new[] { idParams });
                            mmadd.Invoke(materialMany, new [] { obj });
                        }
                    // Remove from hasMany
                    if (mmremove != null)
                        foreach (EntityKey key in materialKeys.Except(ephemeralKeys))
                        {
                            object[] idParams = key.EntityKeyValues.Select(ekv => ekv.Value).ToArray();
                            var obj = (object)await (dynamic) closedGenericGetByIdAsyncMethod.Invoke(this, new[] {idParams});
                            mmremove.Invoke(materialMany, new [] { obj });
                        }
                }
                else if(IsModel(prop.PropertyType))
                {
                    // A belongsTo relationship

                    //if (AuthorizationModel.AllowOperation(AuthorizationOperation.Update, prop, target, newVal))
                    var materialBt = prop.GetValue(material, null);
                    var ephemeralBt = prop.GetValue(ephemeral, null);

                    var materialKey = materialBt == null ? null : MaterializeEntityKey(prop.PropertyType, materialBt);
                    var ephemeralKey = ephemeralBt == null ? null : MaterializeEntityKey(prop.PropertyType, ephemeralBt);

                    if (materialKey != ephemeralKey)
                    {
                        if (ephemeralKey == null)
                        {
                            prop.SetValue(material, null, null);
                        }
                        else
                        {

                            var openGenericGetByIdAsyncMethod = GetType().GetMethod("GetByIdAsync");
                            var closedGenericGetByIdAsyncMethod = openGenericGetByIdAsyncMethod.MakeGenericMethod(prop.PropertyType);

                            object[] idParams = ephemeralKey.EntityKeyValues.Select(ekv => ekv.Value).ToArray();
                            var relatedMaterial = (object)await (dynamic)closedGenericGetByIdAsyncMethod.Invoke(this, new[] { idParams });
                            prop.SetValue(material, relatedMaterial, null);
                        }
                    }
                }
                else
                {
                    object newVal = prop.GetValue(ephemeral);
                    //if (AuthorizationModel.AllowOperation(AuthorizationOperation.Update, prop, target, newVal))
                    if (prop.GetValue(material) != newVal) 
                    {
                        prop.SetValue(material, newVal, null);
                    }
                }
            }

        }
    }
}
