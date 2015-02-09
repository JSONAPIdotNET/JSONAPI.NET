using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSONAPI.EntityFramework.Tests.Models;
using FluentAssertions;
using System.Collections.Generic;

namespace JSONAPI.EntityFramework.Tests
{
    [TestClass]
    public class EntityFrameworkMaterializerTests
    {
        private class NotAnEntity
        {
            public string Id { get; set; }
            public string Temporary { get; set; }
        }

        private TestEntities context;
        private Backlink b1, b2;

        [TestInitialize]
        public void SetupEntities()
        {
            //- See http://stackoverflow.com/a/19130718/489116
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
            //-

            context = new TestEntities();
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
        [ExpectedException(typeof(System.ArgumentException))]
        public void GetKeyNamesNotAnEntityTest()
        {
            // Arrange
            var materializer = new EntityFrameworkMaterializer(context);

            // Act
            IEnumerable<string> keyNames = materializer.GetKeyNames(typeof(NotAnEntity));

            // Assert
            Assert.Fail("A System.ArgumentException should be thrown, this assertion should be unreachable!");
        }
    }
}
