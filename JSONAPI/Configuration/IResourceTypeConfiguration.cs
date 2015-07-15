using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JSONAPI.Core;

namespace JSONAPI.Configuration
{
    /// <summary>
    /// Results of configuring a resource type
    /// </summary>
    public interface IResourceTypeConfiguration
    {
        /// <summary>
        /// The JSON name for this resource type
        /// </summary>
        string ResourceTypeName { get; }

        /// <summary>
        /// The CLR type corresponding to this resource type
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        /// The type of document materializer to use for resources of this type
        /// </summary>
        Type DocumentMaterializerType { get; }

        /// <summary>
        /// A factory to determine the related resource materializer for a given relationship
        /// </summary>
        Func<ResourceTypeRelationship, Type> RelatedResourceMaterializerTypeFactory { get; }

        /// <summary>
        /// Configurations for this type's resources
        /// </summary>
        IDictionary<string, IResourceTypeRelationshipConfiguration> RelationshipConfigurations { get; }

        /// <summary>
        /// A factory to use to build expressions to filter a collection of resources of this type by ID.
        /// </summary>
        Func<ParameterExpression, string, BinaryExpression> FilterByIdExpressionFactory { get; }

        /// <summary>
        /// A factory to use to build expressions to sort a collection of resources of this type by ID.
        /// </summary>
        Func<ParameterExpression, Expression> SortByIdExpressionFactory { get; }

        /// <summary>
        /// Builds a resource type registration corresponding to this type
        /// </summary>
        /// <returns></returns>
        IResourceTypeRegistration BuildResourceTypeRegistration();
    }
}