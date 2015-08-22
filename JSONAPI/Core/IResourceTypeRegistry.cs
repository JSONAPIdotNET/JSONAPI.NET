using System;

namespace JSONAPI.Core
{
    /// <summary>
    /// Manages registrations of this API's resource types.
    /// </summary>
    public interface IResourceTypeRegistry
    {
        /// <summary>
        /// Determines whether a given type has been registered.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>Whether the type is registered</returns>
        bool TypeIsRegistered(Type type);

        /// <summary>
        /// Gets the registration for the given type.
        /// </summary>
        /// <param name="type">The type to get the registration for</param>
        /// <returns>The registration for the given type.</returns>
        /// <exception cref="TypeRegistrationNotFoundException">Thrown when the type was not registered</exception>
        IResourceTypeRegistration GetRegistrationForType(Type type);

        /// <summary>
        /// Gets the registration for the given type.
        /// </summary>
        /// <param name="resourceTypeName">The name of the type to get the registration for</param>
        /// <returns>The registration for the given type name.</returns>
        /// <exception cref="TypeRegistrationNotFoundException">Thrown when the type name was not registered</exception>
        IResourceTypeRegistration GetRegistrationForResourceTypeName(string resourceTypeName);

        /// <summary>
        /// Adds a registration to the registry.
        /// </summary>
        /// <param name="registration">The registration to add</param>
        void AddRegistration(IResourceTypeRegistration registration);
    }
}
