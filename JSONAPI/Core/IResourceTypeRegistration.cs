using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Documents;
using JSONAPI.Http;

namespace JSONAPI.Core
{
    /// <summary>
    /// Represents a registered resource type
    /// </summary>
    public interface IResourceTypeRegistration
    {
        /// <summary>
        /// The resource type's runtime type
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// The property to use to get the ID
        /// </summary>
        PropertyInfo IdProperty { get; }

        /// <summary>
        /// The JSON API name of the resource type
        /// </summary>
        string ResourceTypeName { get; }

        /// <summary>
        /// Gets the attribute fields for the resource type
        /// </summary>
        /// <returns></returns>
        ResourceTypeAttribute[] Attributes { get; }

        /// <summary>
        /// Gets the relationship fields for the resource type
        /// </summary>
        /// <returns></returns>
        ResourceTypeRelationship[] Relationships { get; }

        /// <summary>
        /// Gets the ID for a resource belonging to this resource type
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        string GetIdForResource(object resource);

        /// <summary>
        /// Sets the ID for a resource belonging to this resource type
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="id"></param>
        void SetIdForResource(object resource, string id);

        /// <summary>
        /// Returns an expression that can be used to allow getting an instance of this resource
        /// by ID
        /// </summary>
        /// <returns></returns>
        BinaryExpression GetFilterByIdExpression(ParameterExpression parameter, string id);

        /// <summary>
        /// Returns an expression that can be used to allow sorting this resource by ID.
        /// </summary>
        /// <returns></returns>
        Expression GetSortByIdExpression(ParameterExpression parameter);

        /// <summary>
        /// Gets a field by its JSON API-normalized name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ResourceTypeField GetFieldByName(string name);
    }
}