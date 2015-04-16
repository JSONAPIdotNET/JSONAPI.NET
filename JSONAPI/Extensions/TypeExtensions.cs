using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace JSONAPI.Extensions
{
    public static class TypeExtensions
    {
        public static bool CanWriteAsJsonApiAttribute(this Type objectType)
        {
            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof (Nullable<>))
                objectType = objectType.GetGenericArguments()[0];

            return objectType.IsPrimitive
                   || typeof (Decimal).IsAssignableFrom(objectType)
                   || typeof (Guid).IsAssignableFrom(objectType)
                   || typeof (DateTime).IsAssignableFrom(objectType)
                   || typeof (DateTimeOffset).IsAssignableFrom(objectType)
                   || typeof (String).IsAssignableFrom(objectType)
                   || objectType.IsEnum;
        }

        public static IEnumerable<object> CreateEnumerableInstance(this Type type)
        {
            Type relType;
            if (type.IsGenericType)
            {
                relType = type.GetGenericArguments()[0];
            }
            else
            {
                // Must be an array at this point, right??
                relType = type.GetElementType();
            }

            // Hmm...now we have to create an object that fits this property. This could get messy...
            if (!type.IsInterface && !type.IsAbstract)
            {
                // Whew...okay, just instantiate one of these...
                return (IEnumerable<Object>)Activator.CreateInstance(type);
            }

            // Ugh...now we're really in trouble...hopefully one of these will work:
            if (type.IsGenericType)
            {
                if (type.IsAssignableFrom(typeof(List<>).MakeGenericType(relType)))
                {
                    return (IEnumerable<Object>) Activator.CreateInstance(typeof(List<>).MakeGenericType(relType));
                }

                if (type.IsAssignableFrom(typeof(HashSet<>).MakeGenericType(relType)))
                {
                    return
                        (IEnumerable<Object>) Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(relType));
                }

                //TODO: Other likely candidates??
            }

            return null;
        }
    }
}
