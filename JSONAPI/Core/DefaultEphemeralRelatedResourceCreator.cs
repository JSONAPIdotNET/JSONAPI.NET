using System;

namespace JSONAPI.Core
{
    /// <summary>
    /// Default implementation of <see cref="IEphemeralRelatedResourceCreator"/>, using Activator
    /// </summary>
    public class DefaultEphemeralRelatedResourceCreator : IEphemeralRelatedResourceCreator
    {
        /// <inheritdoc />
        public object CreateEphemeralResource(IResourceTypeRegistration resourceTypeRegistration, string id)
        {
            var obj = Activator.CreateInstance(resourceTypeRegistration.Type);
            resourceTypeRegistration.SetIdForResource(obj, id);
            return obj;
        }
    }
}