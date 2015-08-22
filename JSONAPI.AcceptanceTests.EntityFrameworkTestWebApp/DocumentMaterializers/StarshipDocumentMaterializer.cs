using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models;
using JSONAPI.Core;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using JSONAPI.Http;
using JSONAPI.QueryableTransformers;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.DocumentMaterializers
{
    public class StarshipDocumentMaterializer : MappedDocumentMaterializer<Starship, StarshipDto>
    {
        private readonly TestDbContext _dbContext;

        public StarshipDocumentMaterializer(
            TestDbContext dbContext,
            IQueryableResourceCollectionDocumentBuilder queryableResourceCollectionDocumentBuilder,
            IBaseUrlService baseUrlService, ISingleResourceDocumentBuilder singleResourceDocumentBuilder,
            IQueryableEnumerationTransformer queryableEnumerationTransformer, IResourceTypeRegistry resourceTypeRegistry)
            : base(
                queryableResourceCollectionDocumentBuilder, baseUrlService, singleResourceDocumentBuilder,
                queryableEnumerationTransformer, resourceTypeRegistry)
        {
            _dbContext = dbContext;
        }

        protected override IQueryable<Starship> GetQuery()
        {
            return _dbContext.Starships;
        }

        protected override IQueryable<Starship> GetByIdQuery(string id)
        {
            return GetQuery().Where(s => s.StarshipId == id);
        }

        protected override IQueryable<StarshipDto> GetMappedQuery(IQueryable<Starship> entityQuery, Expression<Func<StarshipDto, object>>[] propertiesToInclude)
        {
            return entityQuery.Select(s => new StarshipDto
            {
                Id = s.StarshipId,
                Name = s.Name,
                StarshipClass = s.StarshipClass.Name
            });
        }

        public override Task<ISingleResourceDocument> CreateRecord(ISingleResourceDocument requestDocument, HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<ISingleResourceDocument> UpdateRecord(string id, ISingleResourceDocument requestDocument, HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<IJsonApiDocument> DeleteRecord(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}