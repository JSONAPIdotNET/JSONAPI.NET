using System;
using System.Collections.Generic;

namespace JSONAPI.Core
{
    /// <summary>
    /// Default implementation of IModelRegistry
    /// </summary>
    public class ResourceTypeRegistry : IResourceTypeRegistry
    {
        private readonly IDictionary<string, IResourceTypeRegistration> _registrationsByName;
        private readonly IDictionary<Type, IResourceTypeRegistration> _registrationsByType; 

        /// <summary>
        /// Creates a new ResourceTypeRegistry
        /// </summary>
        public ResourceTypeRegistry()
        {
            _registrationsByName = new Dictionary<string, IResourceTypeRegistration>();
            _registrationsByType = new Dictionary<Type, IResourceTypeRegistration>();
        }

        public bool TypeIsRegistered(Type type)
        {
            var registration = FindRegistrationForType(type);
            return registration != null;
        }

        public IResourceTypeRegistration GetRegistrationForType(Type type)
        {
            var reg = FindRegistrationForType(type);
            if (reg == null)
                throw new TypeRegistrationNotFoundException(type);

            return reg;
        }

        public IResourceTypeRegistration GetRegistrationForResourceTypeName(string resourceTypeName)
        {
            lock (_registrationsByName)
            {
                IResourceTypeRegistration registration;
                if (!_registrationsByName.TryGetValue(resourceTypeName, out registration))
                    throw new TypeRegistrationNotFoundException(resourceTypeName);

                return registration;
            }
        }

        public void AddRegistration(IResourceTypeRegistration registration)
        {
            lock (_registrationsByType)
            {
                lock (_registrationsByName)
                {
                    if (_registrationsByType.ContainsKey(registration.Type))
                        throw new InvalidOperationException(String.Format("The type `{0}` has already been registered.",
                            registration.Type.FullName));

                    IResourceTypeRegistration existingRegistration;
                    if (_registrationsByName.TryGetValue(registration.ResourceTypeName, out existingRegistration))
                        throw new InvalidOperationException(
                            String.Format("Could not register `{0} under resource type name `{1}` because `{1}` has already been registered by `{2}`.",
                                registration.Type.FullName,
                                registration.ResourceTypeName,
                                existingRegistration.Type.FullName));

                    _registrationsByType.Add(registration.Type, registration);
                    _registrationsByName.Add(registration.ResourceTypeName, registration);
                }
            }
        }

        private IResourceTypeRegistration FindRegistrationForType(Type type)
        {
            lock (_registrationsByType)
            {
                var currentType = type;
                while (currentType != null && currentType != typeof(Object))
                {
                    IResourceTypeRegistration registration;
                    if (_registrationsByType.TryGetValue(currentType, out registration))
                        return registration;

                    // This particular type wasn't registered, but maybe the base type was.
                    currentType = currentType.BaseType;
                }
            }

            return null;
        }
    }
}
