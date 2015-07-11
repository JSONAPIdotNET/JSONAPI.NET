using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class SearchController : ApiController
    {
        private readonly TestDbContext _dbContext;

        public SearchController(TestDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<object>> Get(string s)
        {
            IEnumerable<object> posts = await _dbContext.Posts.Where(p => p.Title.Contains(s)).ToArrayAsync();
            IEnumerable<object> comments = await _dbContext.Comments.Where(p => p.Text.Contains(s)).ToArrayAsync();
            return posts.Concat(comments);
        }
    }
}