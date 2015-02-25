using System;

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

    }
}
