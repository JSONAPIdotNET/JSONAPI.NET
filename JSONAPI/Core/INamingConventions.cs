using System;
using System.Reflection;

namespace JSONAPI.Core
{
    /// <summary>
    /// Allows configuring how to calculate JSON API keys based on CLR types and properties
    /// </summary>
    public interface INamingConventions
    {
        /// <summary>
        /// Calculates the field name for a given property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        string GetFieldNameForProperty(PropertyInfo property);

        /// <summary>
        /// Calculates the resource type name for a CLR type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetResourceTypeNameForType(Type type);
    }
}