using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models;
using JSONAPI.Core;
using JSONAPI.Documents.Builders;
using JSONAPI.EntityFramework.Http;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.DocumentMaterializers
{
    public class StarshipOfficersRelatedResourceMaterializer : EntityFrameworkToManyRelatedResourceDocumentMaterializer<Starship, StarshipOfficerDto>
    {
        private readonly DbContext _dbContext;

        public StarshipOfficersRelatedResourceMaterializer(ResourceTypeRelationship relationship, DbContext dbContext,
            IQueryableResourceCollectionDocumentBuilder queryableResourceCollectionDocumentBuilder,
            IResourceTypeRegistration primaryTypeRegistration)
            : base(relationship, dbContext, queryableResourceCollectionDocumentBuilder, primaryTypeRegistration)
        {
            _dbContext = dbContext;
        }

        protected override Task<IQueryable<StarshipOfficerDto>> GetRelatedQuery(string primaryResourceId, CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<Starship>().Where(s => s.StarshipId == primaryResourceId).SelectMany(s => s.OfficerLinks)
                .Select(l => new StarshipOfficerDto
                {
                    Id = l.StarshipId + "_" + l.OfficerId,
                    Name = l.Officer.Name,
                    Rank = l.Officer.Rank,
                    Position = l.Position
                });
            return Task.FromResult(query);
        }
    }
}