using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models;
using JSONAPI.Core;
using JSONAPI.Documents.Builders;
using JSONAPI.EntityFramework.Http;
using JSONAPI.Http;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.DocumentMaterializers
{
    public class StarshipShipCounselorRelatedResourceMaterializer : EntityFrameworkToOneRelatedResourceDocumentMaterializer<Starship, StarshipOfficerDto>
    {
        private readonly DbContext _dbContext;

        public StarshipShipCounselorRelatedResourceMaterializer(
            ISingleResourceDocumentBuilder singleResourceDocumentBuilder, IBaseUrlService baseUrlService,
            IResourceTypeRegistration primaryTypeRegistration, ResourceTypeRelationship relationship,
            DbContext dbContext)
            : base(singleResourceDocumentBuilder, baseUrlService, primaryTypeRegistration, relationship, dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<StarshipOfficerDto> GetRelatedRecord(string primaryResourceId, CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<Starship>().Where(s => s.StarshipId == primaryResourceId)
                .SelectMany(s => s.OfficerLinks)
                .Where(l => l.Position == "Ship's Counselor")
                .Select(l => new StarshipOfficerDto
                {
                    Id = l.StarshipId + "_" + l.OfficerId,
                    Name = l.Officer.Name,
                    Rank = l.Officer.Rank,
                    Position = l.Position
                });
            return await query.FirstOrDefaultAsync(cancellationToken);
        }
    }
}