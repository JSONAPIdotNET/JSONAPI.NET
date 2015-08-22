using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Http;

namespace JSONAPI.Documents.Builders
{
    /// <summary>
    /// Default implementation of IFallbackDocumentBuilder
    /// </summary>
    public class FallbackDocumentBuilder : IFallbackDocumentBuilder
    {
        private readonly ISingleResourceDocumentBuilder _singleResourceDocumentBuilder;
        private readonly IQueryableResourceCollectionDocumentBuilder _queryableResourceCollectionDocumentBuilder;
        private readonly IResourceCollectionDocumentBuilder _resourceCollectionDocumentBuilder;
        private readonly IBaseUrlService _baseUrlService;
        private readonly Lazy<MethodInfo> _openBuildDocumentFromQueryableMethod;
        private readonly Lazy<MethodInfo> _openBuildDocumentFromEnumerableMethod;

        /// <summary>
        /// Creates a new FallbackDocumentBuilder
        /// </summary>
        public FallbackDocumentBuilder(ISingleResourceDocumentBuilder singleResourceDocumentBuilder,
            IQueryableResourceCollectionDocumentBuilder queryableResourceCollectionDocumentBuilder,
            IResourceCollectionDocumentBuilder resourceCollectionDocumentBuilder,
            IBaseUrlService baseUrlService)
        {
            _singleResourceDocumentBuilder = singleResourceDocumentBuilder;
            _queryableResourceCollectionDocumentBuilder = queryableResourceCollectionDocumentBuilder;
            _resourceCollectionDocumentBuilder = resourceCollectionDocumentBuilder;
            _baseUrlService = baseUrlService;

            _openBuildDocumentFromQueryableMethod =
                new Lazy<MethodInfo>(
                    () => _queryableResourceCollectionDocumentBuilder.GetType()
                        .GetMethod("BuildDocument", BindingFlags.Instance | BindingFlags.Public));

            _openBuildDocumentFromEnumerableMethod =
                new Lazy<MethodInfo>(
                    () => _resourceCollectionDocumentBuilder.GetType()
                        .GetMethod("BuildDocument", BindingFlags.Instance | BindingFlags.Public));
        }

        public async Task<IJsonApiDocument> BuildDocument(object obj, HttpRequestMessage requestMessage,
            CancellationToken cancellationToken)
        {
            var type = obj.GetType();

            var queryableInterfaces = type.GetInterfaces();
            var queryableInterface =
                queryableInterfaces.FirstOrDefault(
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IQueryable<>));
            if (queryableInterface != null)
            {
                var queryableElementType = queryableInterface.GenericTypeArguments[0];
                var buildDocumentMethod =
                    _openBuildDocumentFromQueryableMethod.Value.MakeGenericMethod(queryableElementType);

                dynamic materializedQueryTask = buildDocumentMethod.Invoke(_queryableResourceCollectionDocumentBuilder,
                    new[] { obj, requestMessage, cancellationToken, null });

                return await materializedQueryTask;
            }

            var isCollection = false;
            var enumerableElementType = GetEnumerableElementType(type);
            if (enumerableElementType != null)
            {
                isCollection = true;
            }

            var linkBaseUrl = _baseUrlService.GetBaseUrl(requestMessage);

            if (isCollection)
            {
                var buildDocumentMethod =
                    _openBuildDocumentFromEnumerableMethod.Value.MakeGenericMethod(enumerableElementType);
                return
                    (dynamic)buildDocumentMethod.Invoke(_resourceCollectionDocumentBuilder, new[] { obj, linkBaseUrl, new string[] { }, null });
            }

            // Single resource object
            return _singleResourceDocumentBuilder.BuildDocument(obj, linkBaseUrl, null, null);
        }

        private static Type GetEnumerableElementType(Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType.GetElementType();

            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return collectionType.GetGenericArguments()[0];
            }

            var enumerableInterface = collectionType.GetInterface(typeof(IEnumerable<>).FullName);
            if (enumerableInterface == null) return null;

            var genericArguments = collectionType.GetGenericArguments();
            if (!genericArguments.Any()) return null;

            return genericArguments[0];
        }
    }
}
