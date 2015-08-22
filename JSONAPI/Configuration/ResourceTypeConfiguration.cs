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
    public sealed class ResourceTypeConfiguration<TResourceType> : IResourceTypeConfigurator<TResourceType>, IResourceTypeConfiguration
    {
        private readonly IResourceTypeRegistrar _resourceTypeRegistrar;

        internal ResourceTypeConfiguration(IResourceTypeRegistrar resourceTypeRegistrar)
        {
            _resourceTypeRegistrar = resourceTypeRegistrar;
            RelationshipConfigurations = new ConcurrentDictionary<string, IResourceTypeRelationshipConfiguration>();
            ClrType = typeof (TResourceType);
        }

        public string ResourceTypeName { get; private set; }
        public Type ClrType { get; private set; }
        public Type DocumentMaterializerType { get; private set; }
        public Func<ResourceTypeRelationship, Type> RelatedResourceMaterializerTypeFactory { get; private set; }
        public IDictionary<string, IResourceTypeRelationshipConfiguration> RelationshipConfigurations { get; private set; }
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

            RelationshipConfigurations[propertyInfo.Name] = config;
        }

        public void UseDocumentMaterializer<TMaterializer>()
            where TMaterializer : IDocumentMaterializer
        {
            DocumentMaterializerType = typeof (TMaterializer);
        }

        public void UseDefaultRelatedResourceMaterializer(Func<ResourceTypeRelationship, Type> materializerTypeFactory)
        {
            RelatedResourceMaterializerTypeFactory = materializerTypeFactory;
        }

        public void OverrideDefaultResourceTypeName(string resourceTypeName)
        {
            ResourceTypeName = resourceTypeName;
        }

        public void OverrideDefaultFilterById(Func<ParameterExpression, string, BinaryExpression> filterByIdExpressionFactory)
        {
            FilterByIdExpressionFactory = filterByIdExpressionFactory;
        }

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