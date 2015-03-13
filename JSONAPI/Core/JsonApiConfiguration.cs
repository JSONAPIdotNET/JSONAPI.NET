using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
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
        private IQueryableFilteringTransformer _filteringTransformer;
        private IQueryableSortingTransformer _sortingTransformer;
        private IQueryableEnumerationTransformer _enumerationTransformer;
        private IPluralizationService _pluralizationService;
        private readonly IList<Type> _resourceTypes;

        /// <summary>
        /// Creates a new configuration
        /// </summary>
        public JsonApiConfiguration()
        {
            _enableFiltering = true;
            _enableSorting = true;
            _filteringTransformer = null;
            _sortingTransformer = null;
            _enumerationTransformer = null;
            _resourceTypes = new List<Type>();
        }

        /// <summary>
        /// Disable filtering of IQueryables in GET methods
        /// </summary>
        /// <returns>The same configuration object the method was called on.</returns>
        public JsonApiConfiguration DisableFiltering()
        {
            _enableFiltering = false;
            return this;
        }

        /// <summary>
        /// Disable sorting of IQueryables in GET methods
        /// </summary>
        /// <returns>The same configuration object the method was called on.</returns>
        public JsonApiConfiguration DisableSorting()
        {
            _enableSorting = false;
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
        /// Specifies a service to provide pluralizations of resource type names.
        /// </summary>
        /// <param name="pluralizationService">The service</param>
        /// <returns>The same configuration object that was passed in</returns>
        public JsonApiConfiguration PluralizeResourceTypesWith(IPluralizationService pluralizationService)
        {
            _pluralizationService = pluralizationService;
            return this;
        }

        /// <summary>
        /// Registers a resource type for use with the model manager.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public JsonApiConfiguration RegisterResourceType(Type type)
        {
            _resourceTypes.Add(type);
            return this;
        }

        /// <summary>
        /// Applies the running configuration to an HttpConfiguration instance
        /// </summary>
        /// <param name="httpConfig">The HttpConfiguration to apply this JsonApiConfiguration to</param>
        public void Apply(HttpConfiguration httpConfig)
        {
            var pluralizationService = _pluralizationService ?? new PluralizationService();
            var modelManager = new ModelManager(pluralizationService);
            foreach (var resourceType in _resourceTypes)
            {
                modelManager.RegisterResourceType(resourceType);
            }

            IQueryableFilteringTransformer filteringTransformer = null;
            if (_enableFiltering)
                filteringTransformer = _filteringTransformer ?? new DefaultFilteringTransformer(modelManager);

            IQueryableSortingTransformer sortingTransformer = null;
            if (_enableSorting)
                sortingTransformer = _sortingTransformer ?? new DefaultSortingTransformer(modelManager);

            IQueryableEnumerationTransformer enumerationTransformer =
                _enumerationTransformer ?? new SynchronousEnumerationTransformer();

            var formatter = new JsonApiFormatter(modelManager);

            httpConfig.Formatters.Clear();
            httpConfig.Formatters.Add(formatter);

            httpConfig.Filters.Add(new JsonApiQueryableAttribute(enumerationTransformer, filteringTransformer, sortingTransformer));

            httpConfig.Services.Replace(typeof (IHttpControllerSelector),
                new PascalizedControllerSelector(httpConfig));
        }
    }
}
