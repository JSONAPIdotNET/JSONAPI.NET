using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSONAPI.Core;
using JSONAPI.Tests.Models;
using System.Reflection;

namespace JSONAPI.Tests.Core
{
    [TestClass]
    public class ModelManagerTests
    {
        private class InvalidModel
        {
            public string Data { get; set; }
        }

        [TestMethod]
        public void FindsIdNamedId()
        {
            // Arrange
            var mm = new ModelManager();

            // Act
            PropertyInfo idprop = mm.GetIdProperty(typeof(Author));

            // Assert
            Assert.AreSame(typeof(Author).GetProperty("Id"), idprop);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DoesntFindMissingId()
        {
            // Arrange
            var mm = new ModelManager();

            // Act
            PropertyInfo idprop = mm.GetIdProperty(typeof(InvalidModel));

            // Assert
            Assert.Fail("An InvalidOperationException should be thrown and we shouldn't get here!");
        }
    }
}
