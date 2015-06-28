using System;
using System.Collections.Generic;
using Autofac.Core;
using JSONAPI.Core;
using JSONAPI.Payload;

namespace JSONAPI.Autofac
{
    public class JsonApiAutofacConfiguration
    {
        private readonly INamingConventions _namingConventions;
        private readonly List<Type> _typesToRegister;
        private ILinkConventions _linkConventions;

        public JsonApiAutofacConfiguration(INamingConventions namingConventions)
        {
            if (namingConventions == null) throw new ArgumentNullException("namingConventions");

            _namingConventions = namingConventions;
            _typesToRegister = new List<Type>();
        }

        public void RegisterResourceType(Type resourceType)
        {
            _typesToRegister.Add(resourceType);
        }

        public void OverrideLinkConventions(ILinkConventions linkConventions)
        {
            _linkConventions = linkConventions;
        }

        public IModule GetAutofacModule()
        {
            return new JsonApiAutofacModule(_namingConventions, _linkConventions, _typesToRegister);
        }
    }
}
