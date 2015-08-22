using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Attributes;
using JSONAPI.Core;
using JSONAPI.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Core
{
    [TestClass]
    public class DefaultNamingConventionsTests
    {
        private class Band
        {
            [JsonProperty("THE-GENRE")]
            public string Genre { get; set; }
        }

        private class SomeClass
        {
            public string SomeKey { get; set; }
        }

        [TestMethod]
        public void GetFieldNameForProperty_returns_right_name_for_id()
        {
            // Arrange
            var namingConventions = new DefaultNamingConventions(new PluralizationService());

            // Act
            var name = namingConventions.GetFieldNameForProperty(typeof(Author).GetProperty("Id"));

            // Assert
            name.Should().Be("id");
        }

        [TestMethod]
        public void GetFieldNameForProperty_returns_right_name_for_camel_cased_property()
        {
            // Arrange
            var namingConventions = new DefaultNamingConventions(new PluralizationService());

            // Act
            var name = namingConventions.GetFieldNameForProperty(typeof(SomeClass).GetProperty("SomeKey"));

            // Assert
            name.Should().Be("some-key");
        }

        [TestMethod]
        public void GetFieldNameForProperty_returns_right_name_for_property_with_JsonProperty_attribute()
        {
            // Arrange
            var namingConventions = new DefaultNamingConventions(new PluralizationService());

            // Act
            var name = namingConventions.GetFieldNameForProperty(typeof(Band).GetProperty("Genre"));

            // Assert
            name.Should().Be("THE-GENRE");
        }
    }
}
