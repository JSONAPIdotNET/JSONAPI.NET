using System;
using System.Data.Entity;
using System.Threading.Tasks;
using JSONAPI.Core;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Payload;

namespace JSONAPI.EntityFramework.Tests.TestWebApp
{
    public class CustomEntityFrameworkResourceObjectMaterializer : EntityFrameworkResourceObjectMaterializer
    {
        public CustomEntityFrameworkResourceObjectMaterializer(DbContext dbContext, IResourceTypeRegistry registry) : base(dbContext, registry)
        {
        }

        protected override Task SetIdForNewResource(IResourceObject resourceObject, object newObject, IResourceTypeRegistration typeRegistration)
        {
            // This is to facilitate testing creation of a resource with a server-provided ID
            if (typeRegistration.Type == typeof (Post) && String.IsNullOrEmpty(resourceObject.Id))
            {
                ((Post) newObject).Id = "230";
                return Task.FromResult(0);
            }

            return base.SetIdForNewResource(resourceObject, newObject, typeRegistration);
        }
    }
}