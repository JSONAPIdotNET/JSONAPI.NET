using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using JSONAPI.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Reflection;
using System.Collections;
using System.Data.Entity.Core;

namespace JSONAPI.EntityFramework
{
    public partial class EntityFrameworkMaterializer : IMaterializer
    {
        private DbContext context;

        public DbContext DbContext
        {
            get { return this.context; }
        }

        public EntityFrameworkMaterializer(DbContext context) : base()
        {
            this.context = context;
        }

        #region IMaterializer contract methods

        /// <summary>
        /// Override here if you have mixed models and need to have child objects that are not supposed to be entities!
        /// </summary>
        /// <param name="type"></param>
        /// <param name="idValues"></param>
        /// <returns></returns>
        public virtual object GetById(Type type, params Object[] idValues)
        {
            //TODO: How to react if the type isn't in the context?

            // Input will probably usually be strings... make sure the right types are passed to .Find()...
            Object[] idv2 = new Object[idValues.Length];
            int i = 0;
            foreach (PropertyInfo prop in GetKeyProperties(type))
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
            return this.context.Set(type).Find(idv2);
        }

        public T GetById<T>(params Object[] idValues)
        {
            return (T)GetById(typeof(T), idValues);
        }

        public T Materialize<T>(T ephemeral)
        {
            return (T)Materialize(typeof(T), ephemeral);
        }

        public virtual object Materialize(Type type, object ephemeral)
        {
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
            object retval = null;
            if (!anyNull)
            {
                retval = context.Set(type).Find(idValues.ToArray());
            }
            if (retval == null)
            {
                // Didn't find it...create a new one!
                retval = Activator.CreateInstance(type);
                context.Set(type).Add(retval);
                if (!anyNull)
                {
                    // For a new object, if a key is specified, we want to merge the key, at least.
                    // For simplicity then, make the behavior equivalent to MergeMaterialize in this case.
                    this.Merge(type, ephemeral, retval);
                }
            }
            return retval;
        }

        public T MaterializeUpdate<T>(T ephemeral)
        {
            return (T)MaterializeUpdate(typeof(T), ephemeral);
        }

        public object MaterializeUpdate(Type type, object ephemeral)
        {
            object material = Materialize(type, ephemeral);
            this.Merge(type, ephemeral, material);
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
            if (objectType.IsPrimitive
                || typeof(System.Guid).IsAssignableFrom(objectType)
                || typeof(System.DateTime).IsAssignableFrom(objectType)
                || typeof(System.DateTimeOffset).IsAssignableFrom(objectType)
                || typeof(System.Guid?).IsAssignableFrom(objectType)
                || typeof(System.DateTime?).IsAssignableFrom(objectType)
                || typeof(System.DateTimeOffset?).IsAssignableFrom(objectType)
                || typeof(String).IsAssignableFrom(objectType)
                )
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

        protected virtual IEnumerable<string> GetKeyNames(Type type)
        {
            try
            {
                // If this fails, type must not be in the context!!!
                ObjectContext objectContext = ((IObjectContextAdapter)this.context).ObjectContext;
                IEnumerable<string> retval = (IEnumerable<string>)objectContext.MetadataWorkspace
                    .GetType(type.Name, type.Namespace, System.Data.Entity.Core.Metadata.Edm.DataSpace.CSpace)
                    .MetadataProperties
                    .Where(mp => mp.Name == "KeyMembers")
                    .First()
                    .Value;
                return retval;
            }
            catch (Exception e)
            {
                // Type is not an entity type in the context. Um...now what? Either override this in a subclass, or we'll assume it's "Id"!
                return new string[] { "Id" };
            }
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

        protected string GetEntitySetName(Type type)
        {
            ObjectContext objectContext = ((IObjectContextAdapter)this.context).ObjectContext;
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

        private void Merge (Type type, object ephemeral, object material)
        {
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                // Comply with the spec, if a key was not set, it should not be updated!
                if (!MetadataManager.Instance.PropertyWasPresent(ephemeral, prop)) continue;

                if (IsMany(prop.PropertyType))
                {
                    Type elementType = GetSingleType(prop.PropertyType);
                    IEnumerable<string> keyNames = GetKeyNames(elementType);

                    var materialMany = (IEnumerable<Object>)prop.GetValue(material, null);
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

                    // Add to hasMany
                    if (mmadd != null)
                        foreach (EntityKey key in ephemeralKeys.Except(materialKeys))
                        {
                            object[] idParams = key.EntityKeyValues.Select(ekv => ekv.Value).ToArray();
                            object obj = GetById(elementType, idParams);
                            mmadd.Invoke(materialMany, new object[] { obj });
                        }
                    // Remove from hasMany
                    if (mmremove != null)
                        foreach (EntityKey key in materialKeys.Except(ephemeralKeys))
                        {
                            object[] idParams = key.EntityKeyValues.Select(ekv => ekv.Value).ToArray();
                            object obj = GetById(elementType, idParams);
                            mmremove.Invoke(materialMany, new object[] { obj });
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
                        object[] idParams = ephemeralKey.EntityKeyValues.Select(ekv => ekv.Value).ToArray();
                        prop.SetValue(material, GetById(prop.PropertyType, idParams), null);
                    }
                    // else, 
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
