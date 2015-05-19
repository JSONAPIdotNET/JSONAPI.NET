using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Reflection;
using JSONAPI.Core;

namespace JSONAPI.Http
{
    //TODO: Authorization checking framework, maybe?
    public class ApiController<T> : System.Web.Http.ApiController
        where T : class
    {
        protected virtual IMaterializer MaterializerFactory()
        {
            return null;
        }

        protected virtual TM MaterializerFactory<TM>()
            where TM : IMaterializer
        {
            return (TM)this.MaterializerFactory();
        }

        /// <summary>
        /// Override this method to provide an IQueryable set of objects of type T. If this
        /// method is not overridden, an empty List&lt;T&gt; will be returned.
        /// </summary>
        /// <param name="materializer"></param>
        /// <returns></returns>
        protected virtual IQueryable<T> QueryableFactory(IMaterializer materializer = null)
        {
            return (new List<T>()).AsQueryable<T>();
        }

        //[System.Web.OData.EnableQuery] // Do this yourself!
        /// <summary>
        /// Default Get method implementation. Returns the result of
        /// Note: You can easily add OData query support by overriding this method and decorating
        /// it with the [System.Web.OData.EnableQuery] attribute.
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> Get()
        {
            IMaterializer materializer = MaterializerFactory();

            IQueryable<T> es = QueryableFactory(materializer);

            return es;
        }

        public virtual async Task<IEnumerable<T>> Get(string id)
        {
            IMaterializer materializer = MaterializerFactory();

            List<T> results = new List<T>();
            string[] arrIds;
            if (id.Contains(","))
            {
                 arrIds = id.Split(',');
            }
            else
            {
                arrIds = new string[] { id };
            }
            foreach (string singleid in arrIds)
            {
                T hit = await materializer.GetByIdAsync<T>(singleid);
                if (hit != null)
                {
                    results.Add(hit);
                }
            }
            return results;
        }

        /// <summary>
        /// In this base class, the Post operation is essentially a no-op. It returns a materialized
        /// copy of the object (which is meaningless unless the materializer implements
        /// some logic that does something to it), but fulfills the JSONAPI requirement
        /// that the POST operation return the POSTed object. It should probably be
        /// overridden in any implementation.
        /// </summary>
        /// <param name="postedObj"></param>
        /// <returns></returns>
        public virtual Task<IList<T>> Post([FromBody] IList<T> postedObjs)
        {
            foreach(T postedObj in postedObjs)
            {
                IMaterializer materializer = this.MaterializerFactory();
            }
            return Task.FromResult(postedObjs);
        }

        /// <summary>
        /// Similar to Post, this method doesn't do much. It calls MaterializeUpdateAsync() on the
        /// input and returns it. It should probably always be overridden.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public virtual async Task<IList<T>> Patch(string id, IList<T> putObjs)
        {
            IMaterializer materializer = this.MaterializerFactory();
            IList<T> materialList = new List<T>();
            foreach (T putObj in putObjs)
            {
                materialList.Add(await materializer.MaterializeUpdateAsync<T>(putObj));
            }
            return materialList;
        }

        /// <summary>
        /// A no-op method. This should be overriden in subclasses if Delete is to be supported.
        /// </summary>
        /// <param name="id"></param>
        public virtual Task Delete(string id)
        {
            return Task.FromResult(0);
        }
    }
}
