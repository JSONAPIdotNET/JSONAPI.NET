using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Http;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Default implementation of IFallbackPayloadBuilder
    /// </summary>
    public class FallbackPayloadBuilder : IFallbackPayloadBuilder
    {
        private readonly ISingleResourcePayloadBuilder _singleResourcePayloadBuilder;
        private readonly IQueryableResourceCollectionPayloadBuilder _queryableResourceCollectionPayloadBuilder;
        private readonly IResourceCollectionPayloadBuilder _resourceCollectionPayloadBuilder;
        private readonly IBaseUrlService _baseUrlService;
        private readonly Lazy<MethodInfo> _openBuildPayloadFromQueryableMethod;
        private readonly Lazy<MethodInfo> _openBuildPayloadFromEnumerableMethod;

        /// <summary>
        /// Creates a new FallbackPayloadBuilder
        /// </summary>
        /// <param name="singleResourcePayloadBuilder"></param>
        /// <param name="queryableResourceCollectionPayloadBuilder"></param>
        /// <param name="resourceCollectionPayloadBuilder"></param>
        /// <param name="baseUrlService"></param>
        public FallbackPayloadBuilder(ISingleResourcePayloadBuilder singleResourcePayloadBuilder,
            IQueryableResourceCollectionPayloadBuilder queryableResourceCollectionPayloadBuilder,
            IResourceCollectionPayloadBuilder resourceCollectionPayloadBuilder,
            IBaseUrlService baseUrlService)
        {
            _singleResourcePayloadBuilder = singleResourcePayloadBuilder;
            _queryableResourceCollectionPayloadBuilder = queryableResourceCollectionPayloadBuilder;
            _resourceCollectionPayloadBuilder = resourceCollectionPayloadBuilder;
            _baseUrlService = baseUrlService;

            _openBuildPayloadFromQueryableMethod =
                new Lazy<MethodInfo>(
                    () => _queryableResourceCollectionPayloadBuilder.GetType()
                        .GetMethod("BuildPayload", BindingFlags.Instance | BindingFlags.Public));

            _openBuildPayloadFromEnumerableMethod =
                new Lazy<MethodInfo>(
                    () => _resourceCollectionPayloadBuilder.GetType()
                        .GetMethod("BuildPayload", BindingFlags.Instance | BindingFlags.Public));
        }

        public async Task<IJsonApiPayload> BuildPayload(object obj, HttpRequestMessage requestMessage,
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
                var buildPayloadMethod =
                    _openBuildPayloadFromQueryableMethod.Value.MakeGenericMethod(queryableElementType);

                dynamic materializedQueryTask = buildPayloadMethod.Invoke(_queryableResourceCollectionPayloadBuilder,
                    new[] {obj, requestMessage, cancellationToken});

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
                var buildPayloadMethod =
                    _openBuildPayloadFromEnumerableMethod.Value.MakeGenericMethod(enumerableElementType);
                return
                    (dynamic)buildPayloadMethod.Invoke(_resourceCollectionPayloadBuilder, new[] { obj, linkBaseUrl, new string[] { }, null });
            }

            // Single resource object
            return _singleResourcePayloadBuilder.BuildPayload(obj, linkBaseUrl, null);
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
