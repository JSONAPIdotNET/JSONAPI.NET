using System;
using System.Data.Common;
using System.Linq;
using Effort;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSONAPI.EntityFramework.Tests.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Data.Entity;

namespace JSONAPI.EntityFramework.Tests
{
    [TestClass]
    public class DbContextExtensionsTests
    {
        private class TestDbContext : DbContext
        {
            public DbSet<Backlink> Backlinks { get; set; }
            public DbSet<Post> Posts { get; set; }

            public TestDbContext(DbConnection conn) : base(conn, true)
            {
                
            }
        }

        private class NotAnEntity
        {
            public string Id { get; set; }
            public string Temporary { get; set; }
        }

        private class SubPost : Post
        {
            public string Foo { get; set; }
        }

        private DbConnection _conn;
        private TestDbContext _context;

        [TestInitialize]
        public void SetupEntities()
        {
            _conn = DbConnectionFactory.CreateTransient();
            _context = new TestDbContext(_conn);

            var b1 = new Backlink
            {
                Url = "http://www.google.com/",
                Snippet = "1 Results"
            };
            _context.Backlinks.Add(b1);

            _context.SaveChanges();
        }

        [TestCleanup]
        private void CleanupTest()
        {
            _context.Dispose();
        }

        [TestMethod]
        public void GetKeyNamesStandardIdTest()
        {
            // Act
            IEnumerable<string> keyNames = _context.GetKeyNames(typeof(Post)).ToArray();

            // Assert
            keyNames.Count().Should().Be(1);
            keyNames.First().Should().Be("Id");
        }

        [TestMethod]
        public void GetKeyNamesNonStandardIdTest()
        {
            // Act
            IEnumerable<string> keyNames = _context.GetKeyNames(typeof(Backlink)).ToArray();

            // Assert
            keyNames.Count().Should().Be(1);
            keyNames.First().Should().Be("Url");
        }

        [TestMethod]
        public void GetKeyNamesForChildClass()
        {
            // Act
            IEnumerable<string> keyNames = _context.GetKeyNames(typeof(SubPost)).ToArray();

            // Assert
            keyNames.Count().Should().Be(1);
            keyNames.First().Should().Be("Id");
        }

        [TestMethod]
        public void GetKeyNamesNotAnEntityTest()
        {
            // Act
            Action action = () => _context.GetKeyNames(typeof (NotAnEntity));
            action.ShouldThrow<Exception>().Which.Message.Should().Be("Failed to identify the key names for NotAnEntity or any of its parent classes.");
        }
    }
}
