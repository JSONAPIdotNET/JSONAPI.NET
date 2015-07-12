using System;
using System.Linq.Expressions;

namespace JSONAPI.Core
{
    /// <summary>
    /// Creates resource type registrations based on CLR types
    /// </summary>
    public interface IResourceTypeRegistrar
    {
        /// <summary>
        /// Creates a registration for the given CLR type
        /// </summary>
        IResourceTypeRegistration BuildRegistration(Type type,
            string resourceTypeName = null,
            Func<ParameterExpression, string, BinaryExpression> filterByIdFactory = null,
            Func<ParameterExpression, Expression> sortByIdFactory = null);
    }
}