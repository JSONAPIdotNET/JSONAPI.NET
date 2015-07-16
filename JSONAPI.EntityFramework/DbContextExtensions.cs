using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace JSONAPI.EntityFramework
{
    /// <summary>
    /// Extensions on DbContext useful for JSONAPI.NET
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Gets the ID key names for an entity type
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetKeyNames(this DbContext dbContext, Type type)
        {
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            if (type == null) throw new ArgumentNullException("type");

            var originalType = type;

            while (type != null)
            {
                var openMethod = typeof(DbContextExtensions).GetMethod("GetKeyNamesFromGeneric", BindingFlags.Public | BindingFlags.Static);
                var method = openMethod.MakeGenericMethod(type);

                try
                {
                    return (IEnumerable<string>) method.Invoke(null, new object[] {dbContext});
                }
                catch (TargetInvocationException)
                {
                }

                type = type.BaseType;
            }

            throw new Exception(string.Format("Failed to identify the key names for {0} or any of its parent classes.", originalType.Name));
        }

        /// <summary>
        /// Gets the ID key names for an entity type
        /// </summary>
        /// <param name="dbContext"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<string> GetKeyNamesFromGeneric<T>(this DbContext dbContext) where T : class
        {
            var objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;
            ObjectSet<T> objectSet;
            try
            {
                objectSet = objectContext.CreateObjectSet<T>();

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

    }
}
