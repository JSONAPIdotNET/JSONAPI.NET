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

                    if (_registrationsByName.ContainsKey(registration.ResourceTypeName))
                        throw new InvalidOperationException(
                            String.Format("The resource type name `{0}` has already been registered.",
                                registration.ResourceTypeName));

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
