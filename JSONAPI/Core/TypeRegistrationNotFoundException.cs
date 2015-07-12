using System;

namespace JSONAPI.Core
{
    /// <summary>
    /// Exception thrown when a model is looked up for a type that has not been registred
    /// </summary>
    public class TypeRegistrationNotFoundException : Exception
    {
        /// <summary>
        /// Creates a ModelRegistrationNotFoundException for a type lookup failure
        /// </summary>
        /// <param name="type"></param>
        public TypeRegistrationNotFoundException(Type type)
            : base("No type registration was found for the type \"" + type.Name + "\".")
        {
        }

        /// <summary>
        /// Creates a ModelRegistrationNotFoundException for a resource type name lookup failure
        /// </summary>
        /// <param name="resourceTypeName"></param>
        public TypeRegistrationNotFoundException(string resourceTypeName)
            : base("No type registration was found for the type name \"" + resourceTypeName + "\".")
        {
        }
    }
}