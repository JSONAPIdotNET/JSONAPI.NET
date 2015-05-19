using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using JSONAPI.ActionFilters;
using JSONAPI.Http;
using JSONAPI.Json;

namespace JSONAPI.Core
{
    /// <summary>
    /// Configuration API for JSONAPI.NET
    /// </summary>
    public class JsonApiConfiguration
    {
        private bool _enableFiltering;
        private bool _enableSorting;
        private bool _enablePagination;
        private IQueryableFilteringTransformer _filteringTransformer;
        private IQueryableSortingTransformer _sortingTransformer;
        private IQueryablePaginationTransformer _paginationTransformer;
        private IQueryableEnumerationTransformer _enumerationTransformer;
        private readonly IModelManager _modelManager;

        /// <summary>
        /// Creates a new configuration
        /// </summary>
        public JsonApiConfiguration(IModelManager modelManager)
        {
            if (modelManager == null) throw new Exception("You must provide ");

            _enableFiltering = true;
            _enableSorting = true;
            _enablePagination = true;
            _filteringTransformer = null;
            _sortingTransformer = null;
            _paginationTransformer = null;
            _enumerationTransformer = null;
            _modelManager = modelManager;
        }

        /// <summary>
        /// Disables filtering of IQueryables in GET methods
        /// </summary>
        /// <returns>The same configuration object the method was called on.</returns>
        public JsonApiConfiguration DisableFiltering()
        {
            _enableFiltering = false;
            return this;
        }

        /// <summary>
        /// Disables sorting of IQueryables in GET methods
        /// </summary>
        /// <returns>The same configuration object the method was called on.</returns>
        public JsonApiConfiguration DisableSorting()
        {
            _enableSorting = false;
            return this;
        }

        /// <summary>
        /// Disables pagination of IQueryables in GET methods
        /// </summary>
        /// <returns>The same configuration object the method was called on.</returns>
        public JsonApiConfiguration DisablePagination()
        {
            _enablePagination = false;
            return this;
        }

        /// <summary>
        /// Specifies a filtering transformer to use for filtering IQueryable response payloads.
        /// </summary>
        /// <param name="filteringTransformer">The filtering transformer.</param>
        /// <returns>The same configuration object the method was called on.</returns>
        public JsonApiConfiguration FilterWith(IQueryableFilteringTransformer filteringTransformer)
        {
            _filteringTransformer = filteringTransformer;
            return this;
        }

        /// <summary>
        /// Specifies a sorting transformer to use for sorting IQueryable response payloads.
        /// </summary>
        /// <param name="sortingTransformer">The sorting transformer.</param>
        /// <returns>The same configuration object the method was called on.</returns>
        public JsonApiConfiguration SortWith(IQueryableSortingTransformer sortingTransformer)
        {
            _sortingTransformer = sortingTransformer;
            return this;
        }

        /// <summary>
        /// Specifies a pagination transformer to use for paging IQueryable response payloads.
        /// </summary>
        /// <param name="paginationTransformer">The pagination transformer.</param>
        /// <returns>The same configuration object the method was called on.</returns>
        public JsonApiConfiguration PageWith(IQueryablePaginationTransformer paginationTransformer)
        {
            _paginationTransformer = paginationTransformer;
            return this;
        }

        /// <summary>
        /// Specifies an enumeration transformer to use for enumerating IQueryable response payloads.
        /// </summary>
        /// <param name="enumerationTransformer">The enumeration transformer.</param>
        /// <returns>The same configuration object the method was called on.</returns>
        public JsonApiConfiguration EnumerateQueriesWith(IQueryableEnumerationTransformer enumerationTransformer)
        {
            _enumerationTransformer = enumerationTransformer;
            return this;
        }

        /// <summary>
        /// Applies the running configuration to an HttpConfiguration instance
        /// </summary>
        /// <param name="httpConfig">The HttpConfiguration to apply this JsonApiConfiguration to</param>
        public void Apply(HttpConfiguration httpConfig)
        {
            IQueryableFilteringTransformer filteringTransformer = null;
            if (_enableFiltering)
                filteringTransformer = _filteringTransformer ?? new DefaultFilteringTransformer(_modelManager);

            IQueryableSortingTransformer sortingTransformer = null;
            if (_enableSorting)
                sortingTransformer = _sortingTransformer ?? new DefaultSortingTransformer(_modelManager);

            IQueryablePaginationTransformer paginationTransformer = null;
            if (_enablePagination)
                paginationTransformer =
                    _paginationTransformer ?? new DefaultPaginationTransformer("page.number", "page.size", null);

            IQueryableEnumerationTransformer enumerationTransformer =
                _enumerationTransformer ?? new SynchronousEnumerationTransformer();

            var formatter = new JsonApiFormatter(_modelManager);

            httpConfig.Formatters.Clear();
            httpConfig.Formatters.Add(formatter);

            httpConfig.Filters.Add(new JsonApiQueryableAttribute(enumerationTransformer, filteringTransformer, sortingTransformer, paginationTransformer));

            httpConfig.Services.Replace(typeof (IHttpControllerSelector),
                new PascalizedControllerSelector(httpConfig));
        }
    }
}
