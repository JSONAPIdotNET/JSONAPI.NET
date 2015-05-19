using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSONAPI.Core;

namespace JSONAPI.EntityFramework.Http
{
    public class ApiController<T,TC> : JSONAPI.Http.ApiController<T>
        where T : class // hmm...see http://stackoverflow.com/a/6451237/489116
        where TC : DbContext
    {
        private EntityFrameworkMaterializer _materializer = null;

        protected override JSONAPI.Core.IMaterializer MaterializerFactory()
        {
            if (_materializer == null)
            {
                DbContext context = (DbContext)Activator.CreateInstance(typeof(TC));
                var metadataManager = MetadataManager.Instance;
                _materializer = new JSONAPI.EntityFramework.EntityFrameworkMaterializer(context, metadataManager);
            }
            return _materializer;
        }

        protected override TM MaterializerFactory<TM>()
        {
            return base.MaterializerFactory<TM>();
        }

        protected override IQueryable<T> QueryableFactory(Core.IMaterializer materializer = null)
        {
            if (materializer == null)
            {
                materializer = MaterializerFactory();
            }
            return ((EntityFrameworkMaterializer)materializer).DbContext.Set<T>();
        }

        public override async Task<IList<T>> Post(IList<T> postedObjs)
        {
            var materializer = this.MaterializerFactory<EntityFrameworkMaterializer>();
            List<T> materialList = new List<T>();
            foreach (T postedObj in postedObjs)
            {
                DbContext context = materializer.DbContext;
                var material = await materializer.MaterializeUpdateAsync(postedObj);
                if (context.Entry<T>(material).State == EntityState.Added)
                {
                    await context.SaveChangesAsync();
                    materialList.Add(material);
                }
                else
                {
                    // POST should only create an object--if the EntityState is Unchanged or Modified, this is an illegal operation.
                    var e = new System.Web.Http.HttpResponseException(System.Net.HttpStatusCode.BadRequest);
                    //e.InnerException = new ArgumentException("The POSTed object already exists!"); // Can't do this, I guess...
                    throw e;
                }
            }
            return materialList;
        }

        public override async Task<IList<T>> Patch(string id, IList<T> putObjs)
        {
            var materializer = this.MaterializerFactory<EntityFrameworkMaterializer>();
            DbContext context = materializer.DbContext;
            List<T> materialList = new List<T>();
            foreach (T putObj in putObjs)
            {
                var material = await materializer.MaterializeUpdateAsync(putObj);
                materialList.Add(material);
            }
            await context.SaveChangesAsync();
            return materialList;
        }

        public override async Task Delete(string id)
        {
            var materializer = this.MaterializerFactory<EntityFrameworkMaterializer>();
            DbContext context = materializer.DbContext;
            T target = await materializer.GetByIdAsync<T>(id);
            context.Set<T>().Remove(target);
            await context.SaveChangesAsync();
            await base.Delete(id);
        }

        protected override void Dispose(bool disposing)
        {
            //FIXME: Unsure what to do with the "disposing" parameter here...what does it mean??
            if (_materializer != null)
            {
                _materializer.DbContext.Dispose();
            }
            _materializer = null;
            base.Dispose(disposing);
        }
    }
}
