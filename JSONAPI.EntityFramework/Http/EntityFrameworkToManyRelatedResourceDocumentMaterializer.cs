using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Core;
using JSONAPI.Documents.Builders;
using JSONAPI.Http;

namespace JSONAPI.EntityFramework.Http
{
    /// <summary>
    /// Implementation of <see cref="JSONAPI.Http.IRelatedResourceDocumentMaterializer"/> for use with Entity Framework
    /// </summary>
    public class EntityFrameworkToManyRelatedResourceDocumentMaterializer<TPrimaryResource, TRelated> :
        QueryableToManyRelatedResourceDocumentMaterializer<TRelated> where TPrimaryResource : class
    {
        private readonly IResourceTypeRegistration _primaryTypeRegistration;
        private readonly ResourceTypeRelationship _relationship;
        private readonly DbContext _dbContext;

        /// <summary>
        /// Builds a new EntityFrameworkToManyRelatedResourceDocumentMaterializer.
        /// </summary>
        public EntityFrameworkToManyRelatedResourceDocumentMaterializer(
            ResourceTypeRelationship relationship,
            DbContext dbContext,
            IQueryableResourceCollectionDocumentBuilder queryableResourceCollectionDocumentBuilder,
            IResourceTypeRegistration primaryTypeRegistration)
            : base(queryableResourceCollectionDocumentBuilder)
        {
            _relationship = relationship;
            _dbContext = dbContext;
            _primaryTypeRegistration = primaryTypeRegistration;
        }

        protected override async Task<IQueryable<TRelated>> GetRelatedQuery(string primaryResourceId,
            CancellationToken cancellationToken)
        {
            var param = Expression.Parameter(typeof (TPrimaryResource));
            var accessorExpr = Expression.Property(param, _relationship.Property);
            var lambda = Expression.Lambda<Func<TPrimaryResource, IEnumerable<TRelated>>>(accessorExpr, param);

            var primaryEntityQuery = FilterById<TPrimaryResource>(primaryResourceId, _primaryTypeRegistration);

            // We have to see if the resource even exists, so we can throw a 404 if it doesn't
            var relatedResource = await primaryEntityQuery.FirstOrDefaultAsync(cancellationToken);
            if (relatedResource == null)
                throw JsonApiException.CreateForNotFound(string.Format(
                    "No resource of type `{0}` exists with id `{1}`.",
                    _primaryTypeRegistration.ResourceTypeName, primaryResourceId));

            return primaryEntityQuery.SelectMany(lambda);
        }

        private IQueryable<TResource> FilterById<TResource>(string id,
            IResourceTypeRegistration resourceTypeRegistration,
            params Expression<Func<TResource, object>>[] includes) where TResource : class
        {
            var param = Expression.Parameter(typeof (TResource));
            var filterByIdExpression = resourceTypeRegistration.GetFilterByIdExpression(param, id);
            var predicate = Expression.Lambda<Func<TResource, bool>>(filterByIdExpression, param);
            return Filter(predicate, includes);
        }

        private IQueryable<TResource> Filter<TResource>(Expression<Func<TResource, bool>> predicate,
            params Expression<Func<TResource, object>>[] includes) where TResource : class
        {
            IQueryable<TResource> query = _dbContext.Set<TResource>();
            if (includes != null && includes.Any())
                query = includes.Aggregate(query, (current, include) => current.Include(include));

            if (predicate != null)
                query = query.Where(predicate);

            return query.AsQueryable();
        }
    }
}
