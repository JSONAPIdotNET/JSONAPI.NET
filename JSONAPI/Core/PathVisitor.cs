using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JSONAPI.Core
{
    /// <summary>
    /// Utility for converting an property expression into a dot-separated path string
    /// </summary>
    public class PathVisitor : ExpressionVisitor
    {
        private readonly IResourceTypeRegistry _resourceTypeRegistry;

        /// <summary>
        /// Creates a new PathVisitor
        /// </summary>
        /// <param name="resourceTypeRegistry"></param>
        public PathVisitor(IResourceTypeRegistry resourceTypeRegistry)
        {
            _resourceTypeRegistry = resourceTypeRegistry;
        }

        private readonly Stack<string> _segments = new Stack<string>();
        public string Path { get { return string.Join(".", _segments.ToArray()); } }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "Select")
            {
                Visit(node.Arguments[1]);
                Visit(node.Arguments[0]);
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var property = node.Member as PropertyInfo;
            if (property == null) return node;

            var registration = _resourceTypeRegistry.GetRegistrationForType(property.DeclaringType);
            if (registration == null || registration.Relationships == null) return node;

            var relationship = registration.Relationships.FirstOrDefault(r => r.Property == property);
            if (relationship == null) return node;

            _segments.Push(relationship.JsonKey);

            return base.VisitMember(node);
        }
    }
}
