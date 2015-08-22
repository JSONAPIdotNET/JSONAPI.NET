using System;
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
    public class EntityFrameworkToOneRelatedResourceDocumentMaterializer<TPrimaryResource, TRelated> :
        QueryableToOneRelatedResourceDocumentMaterializer<TRelated> where TPrimaryResource : class
    {
        private readonly IResourceTypeRegistration _primaryTypeRegistration;
        private readonly ResourceTypeRelationship _relationship;
        private readonly DbContext _dbContext;

        /// <summary>
        /// Builds a new EntityFrameworkToOneRelatedResourceDocumentMaterializer
        /// </summary>
        public EntityFrameworkToOneRelatedResourceDocumentMaterializer(
            ISingleResourceDocumentBuilder singleResourceDocumentBuilder, IBaseUrlService baseUrlService,
            IResourceTypeRegistration primaryTypeRegistration, ResourceTypeRelationship relationship,
            DbContext dbContext)
            : base(singleResourceDocumentBuilder, baseUrlService)
        {
            _primaryTypeRegistration = primaryTypeRegistration;
            _relationship = relationship;
            _dbContext = dbContext;
        }

        protected override async Task<TRelated> GetRelatedRecord(string primaryResourceId, CancellationToken cancellationToken)
        {
            var param = Expression.Parameter(typeof(TPrimaryResource));
            var accessorExpr = Expression.Property(param, _relationship.Property);
            var lambda = Expression.Lambda<Func<TPrimaryResource, TRelated>>(accessorExpr, param);

            var primaryEntityQuery = FilterById<TPrimaryResource>(primaryResourceId, _primaryTypeRegistration);
            var primaryEntityExists = await primaryEntityQuery.AnyAsync(cancellationToken);
            if (!primaryEntityExists)
                throw JsonApiException.CreateForNotFound(string.Format("No resource of type `{0}` exists with id `{1}`.",
                    _primaryTypeRegistration.ResourceTypeName, primaryResourceId));
            return await primaryEntityQuery.Select(lambda).FirstOrDefaultAsync(cancellationToken);
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

        private IQueryable<TResource> FilterById<TResource>(string id, IResourceTypeRegistration resourceTypeRegistration,
            params Expression<Func<TResource, object>>[] includes) where TResource : class
        {
            var param = Expression.Parameter(typeof(TResource));
            var filterByIdExpression = resourceTypeRegistration.GetFilterByIdExpression(param, id);
            var predicate = Expression.Lambda<Func<TResource, bool>>(filterByIdExpression, param);
            return Filter(predicate, includes);
        }
    }
}
