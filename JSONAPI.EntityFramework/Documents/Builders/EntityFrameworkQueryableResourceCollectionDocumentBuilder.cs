using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using JSONAPI.Http;
using JSONAPI.QueryableTransformers;

namespace JSONAPI.EntityFramework.Documents.Builders
{
    /// <summary>
    /// Provides a entity framework implementation of an IQueryableResourceCollectionDocumentBuilder
    /// </summary>
    public class EntityFrameworkQueryableResourceCollectionDocumentBuilder: DefaultQueryableResourceCollectionDocumentBuilder
    {
        /// <summary>
        /// Creates a new EntityFrameworkQueryableResourceCollectionDocumentBuilder
        /// </summary>
        public EntityFrameworkQueryableResourceCollectionDocumentBuilder(
            IResourceCollectionDocumentBuilder resourceCollectionDocumentBuilder,
            IQueryableEnumerationTransformer enumerationTransformer,
            IQueryableFilteringTransformer filteringTransformer,
            IQueryableSortingTransformer sortingTransformer,
            IQueryablePaginationTransformer paginationTransformer,
            IBaseUrlService baseUrlService) :
            base(resourceCollectionDocumentBuilder,
                enumerationTransformer,
                filteringTransformer,
                sortingTransformer,
                paginationTransformer,
                baseUrlService)
        {
        }

        /// <summary>
        /// Returns the metadata that should be sent with this document.
        /// </summary>
        protected override async Task<IMetadata> GetDocumentMetadata<T>(IQueryable<T> originalQuery, IQueryable<T> filteredQuery, IOrderedQueryable<T> sortedQuery,
            IPaginationTransformResult<T> paginationResult, CancellationToken cancellationToken)
        {
            var metadata = new Metadata();
            if (paginationResult.PaginationWasApplied)
            {
                var count = await filteredQuery.CountAsync(cancellationToken);
                metadata.MetaObject.Add("total-pages", (int)Math.Ceiling((decimal) count / paginationResult.PageSize));
                metadata.MetaObject.Add("total-count", count);
            }
            if (metadata.MetaObject.HasValues)
                return metadata;
            return null;
        }
    }
}
