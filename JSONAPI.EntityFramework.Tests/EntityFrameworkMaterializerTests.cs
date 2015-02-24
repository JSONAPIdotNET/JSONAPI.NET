using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSONAPI.EntityFramework.Tests.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Data.Entity;

namespace JSONAPI.EntityFramework.Tests
{
    [TestClass]
    public class EntityFrameworkMaterializerTests
    {
        private class TestDbContext : DbContext
        {
            public DbSet<Backlink> Backlinks { get; set; }
            public DbSet<Post> Posts { get; set; }
        }

        private class NotAnEntity
        {
            public string Id { get; set; }
            public string Temporary { get; set; }
        }

        private TestDbContext context;
        private Backlink b1, b2;

        [TestInitialize]
        public void SetupEntities()
        {
            //- See http://stackoverflow.com/a/19130718/489116
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
            //-

            context = new TestDbContext();
            //JSONAPI.EntityFramework.Json.ContractResolver.ObjectContext = context;


            // Clear it out!
            foreach (Backlink o in context.Backlinks) context.Backlinks.Remove(o);
            context.SaveChanges();

            b1 = new Backlink
            {
                Url = "http://www.google.com/",
                Snippet = "1 Results"
            };

            context.SaveChanges();
        }

        [TestMethod]
        public void GetKeyNamesStandardIdTest()
        {
            // Arrange
            var materializer = new EntityFrameworkMaterializer(context);

            // Act
            IEnumerable<string> keyNames = materializer.GetKeyNames(typeof(Post));

            // Assert
            keyNames.Count().Should().Be(1);
            keyNames.First().Should().Be("Id");
        }

        [TestMethod]
        public void GetKeyNamesNonStandardIdTest()
        {
            // Arrange
            var materializer = new EntityFrameworkMaterializer(context);

            // Act
            IEnumerable<string> keyNames = materializer.GetKeyNames(typeof(Backlink));

            // Assert
            keyNames.Count().Should().Be(1);
            keyNames.First().Should().Be("Url");
        }

        [TestMethod]
        public void GetKeyNamesNotAnEntityTest()
        {
            // Arrange
            var materializer = new EntityFrameworkMaterializer(context);

            // Act
            Action action = () =>
            {
                materializer.GetKeyNames(typeof (NotAnEntity));
            };
            action.ShouldThrow<ArgumentException>().Which.Message.Should().Be("The Type NotAnEntity was not found in the DbContext with Type TestDbContext");
        }
    }
}
