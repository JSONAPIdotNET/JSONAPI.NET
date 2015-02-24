using JSONAPI.Core;
using JSONAPI.EntityFramework.Http;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class UserGroupsController : ApiController<UserGroup, TestDbContext>
    {
        protected readonly TestDbContext DbContext;

        public UserGroupsController(TestDbContext dbContext)
        {
            DbContext = dbContext;
        }

        protected override IMaterializer MaterializerFactory()
        {
            return new EntityFrameworkMaterializer(DbContext, MetadataManager.Instance);
        }
    }
}