using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Autofac.Core;
using JSONAPI.Core;
using JSONAPI.Documents;

namespace JSONAPI.Autofac
{
    public class JsonApiAutofacConfiguration
    {
        private readonly INamingConventions _namingConventions;
        private readonly List<Action<ResourceTypeRegistry>> _registrationActions;
        private ILinkConventions _linkConventions;

        public JsonApiAutofacConfiguration(INamingConventions namingConventions)
        {
            if (namingConventions == null) throw new ArgumentNullException("namingConventions");

            _namingConventions = namingConventions;
            _registrationActions = new List<Action<ResourceTypeRegistry>>();
        }

        public void RegisterResourceType(Type resourceType, string resourceTypeName = null,
            Func<ParameterExpression, string, BinaryExpression> filterByIdFactory = null, Func<ParameterExpression, Expression> sortByIdFactory = null)
        {
            _registrationActions.Add(
                registry => registry.RegisterResourceType(resourceType, resourceTypeName, filterByIdFactory, sortByIdFactory));
        }

        public void OverrideLinkConventions(ILinkConventions linkConventions)
        {
            _linkConventions = linkConventions;
        }

        public IModule GetAutofacModule()
        {
            return new JsonApiAutofacModule(_namingConventions, _linkConventions, _registrationActions);
        }
    }
}
