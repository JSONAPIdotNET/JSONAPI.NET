using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JSONAPI.Core;
using JSONAPI.Http;

namespace JSONAPI.Configuration
{
    /// <summary>
    /// Configuration mechanism for resource types.
    /// </summary>
    /// <typeparam name="TResourceType"></typeparam>
    public sealed class ResourceTypeConfiguration<TResourceType> : IResourceTypeConfiguration, IResourceTypeConfigurator<TResourceType>
    {
        private readonly IResourceTypeRegistrar _resourceTypeRegistrar;

        internal ResourceTypeConfiguration(IResourceTypeRegistrar resourceTypeRegistrar)
        {
            _resourceTypeRegistrar = resourceTypeRegistrar;
            RelationshipConfigurations = new ConcurrentDictionary<PropertyInfo, IResourceTypeRelationshipConfiguration>();
            ClrType = typeof (TResourceType);
        }

        public string ResourceTypeName { get; private set; }
        public Type ClrType { get; private set; }
        public Type DocumentMaterializerType { get; private set; }
        public IDictionary<PropertyInfo, IResourceTypeRelationshipConfiguration> RelationshipConfigurations { get; private set; }
        public Func<ParameterExpression, string, BinaryExpression> FilterByIdExpressionFactory { get; private set; }
        public Func<ParameterExpression, Expression> SortByIdExpressionFactory { get; private set; }

        public void ConfigureRelationship(Expression<Func<TResourceType, object>> property,
            Action<IResourceTypeRelationshipConfigurator> configurationAction)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (configurationAction == null) throw new ArgumentNullException("configurationAction");

            var member = (MemberExpression) property.Body;
            var propertyInfo = (PropertyInfo) member.Member;

            var config = new ResourceTypeRelationshipConfiguration();
            configurationAction(config);

            RelationshipConfigurations[propertyInfo] = config;
        }

        public void UseDocumentMaterializer<TMaterializer>()
            where TMaterializer : IDocumentMaterializer
        {
            DocumentMaterializerType = typeof (TMaterializer);
        }

        /// <summary>
        /// Overrides the resource type name from naming conventions
        /// </summary>
        /// <param name="resourceTypeName"></param>
        public void OverrideDefaultResourceTypeName(string resourceTypeName)
        {
            ResourceTypeName = resourceTypeName;
        }

        /// <summary>
        /// Specifies a function to use build expressions that allow filtering resources of this type by ID
        /// </summary>
        public void OverrideDefaultFilterById(Func<ParameterExpression, string, BinaryExpression> filterByIdExpressionFactory)
        {
            FilterByIdExpressionFactory = filterByIdExpressionFactory;
        }

        /// <summary>
        /// Specifies a function to use build expressions that allow sorting resources of this type by ID
        /// </summary>
        public void OverrideDefaultSortById(Func<ParameterExpression, Expression> sortByIdExpressionFactory)
        {
            SortByIdExpressionFactory = sortByIdExpressionFactory;
        }

        public IResourceTypeRegistration BuildResourceTypeRegistration()
        {
            return _resourceTypeRegistrar.BuildRegistration(ClrType, ResourceTypeName, FilterByIdExpressionFactory,
                SortByIdExpressionFactory);
        }
    }
}