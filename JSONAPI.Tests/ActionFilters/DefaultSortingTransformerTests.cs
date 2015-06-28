using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using JSONAPI.ActionFilters;
using JSONAPI.Core;
using JSONAPI.Payload.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.ActionFilters
{
    [TestClass]
    public class DefaultSortingTransformerTests : QueryableTransformerTestsBase
    {
        private class Dummy
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public string Id { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local

            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        private IList<Dummy> _fixtures;
        private IQueryable<Dummy> _fixturesQuery;

        [TestInitialize]
        public void SetupFixtures()
        {
            _fixtures = new List<Dummy>
            {
                new Dummy { Id = "1", FirstName = "Thomas", LastName = "Paine" },
                new Dummy { Id = "2", FirstName = "Samuel", LastName = "Adams" },
                new Dummy { Id = "3", FirstName = "George", LastName = "Washington"},
                new Dummy { Id = "4", FirstName = "Thomas", LastName = "Jefferson" },
                new Dummy { Id = "5", FirstName = "Martha", LastName = "Washington"},
                new Dummy { Id = "6", FirstName = "Abraham", LastName = "Lincoln" },
                new Dummy { Id = "7", FirstName = "Andrew", LastName = "Jackson" },
                new Dummy { Id = "8", FirstName = "Andrew", LastName = "Johnson" },
                new Dummy { Id = "9", FirstName = "William", LastName = "Harrison" }
            };
            _fixturesQuery = _fixtures.AsQueryable();
        }

        private DefaultSortingTransformer GetTransformer()
        {
            var pluralizationService = new PluralizationService(new Dictionary<string, string>
            {
                {"Dummy", "Dummies"}
            });
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(pluralizationService));
            registry.RegisterResourceType(typeof(Dummy));
            return new DefaultSortingTransformer(registry);
        }

        private Dummy[] GetArray(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return GetTransformer().Sort(_fixturesQuery, request).ToArray();
        }

        private void RunTransformAndExpectFailure(string uri, string expectedMessage)
        {
            Action action = () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, uri);

                // ReSharper disable once UnusedVariable
                var result = GetTransformer().Sort(_fixturesQuery, request).ToArray();
            };
            action.ShouldThrow<JsonApiException>().Which.Error.Detail.Should().Be(expectedMessage);
        }

        [TestMethod]
        public void Sorts_by_attribute_ascending()
        {
            var array = GetArray("http://api.example.com/dummies?sort=%2Bfirst-name");
            array.Should().BeInAscendingOrder(d => d.FirstName);
        }

        [TestMethod]
        public void Sorts_by_attribute_descending()
        {
            var array = GetArray("http://api.example.com/dummies?sort=-first-name");
            array.Should().BeInDescendingOrder(d => d.FirstName);
        }

        [TestMethod]
        public void Sorts_by_two_ascending_attributes()
        {
            var array = GetArray("http://api.example.com/dummies?sort=%2Blast-name,%2Bfirst-name");
            array.Should().ContainInOrder(_fixtures.OrderBy(d => d.LastName + d.FirstName));
        }

        [TestMethod]
        public void Sorts_by_two_descending_attributes()
        {
            var array = GetArray("http://api.example.com/dummies?sort=-last-name,-first-name");
            array.Should().ContainInOrder(_fixtures.OrderByDescending(d => d.LastName + d.FirstName));
        }

        [TestMethod]
        public void Returns_400_if_sort_argument_is_empty()
        {
            RunTransformAndExpectFailure("http://api.example.com/dummies?sort=", "The sort expression \"\" is invalid.");
        }

        [TestMethod]
        public void Returns_400_if_sort_argument_is_whitespace()
        {
            RunTransformAndExpectFailure("http://api.example.com/dummies?sort= ", "The sort expression \"\" is invalid.");
        }

        [TestMethod]
        public void Returns_400_if_property_name_is_missing()
        {
            RunTransformAndExpectFailure("http://api.example.com/dummies?sort=%2B", "One of the sort expressions is empty.");
        }

        [TestMethod]
        public void Returns_400_if_property_name_is_whitespace()
        {
            RunTransformAndExpectFailure("http://api.example.com/dummies?sort=%2B ", "One of the sort expressions is empty.");
        }

        [TestMethod]
        public void Returns_400_if_no_property_exists()
        {
            RunTransformAndExpectFailure("http://api.example.com/dummies?sort=%2Bfoobar", "The attribute \"foobar\" does not exist on type \"dummies\".");
        }

        [TestMethod]
        public void Returns_400_if_the_same_property_is_specified_more_than_once()
        {
            RunTransformAndExpectFailure("http://api.example.com/dummies?sort=%2Blast-name,%2Blast-name", "The attribute \"last-name\" was specified more than once.");
        }

        [TestMethod]
        public void Returns_400_if_sort_argument_doesnt_start_with_plus_or_minus()
        {
            RunTransformAndExpectFailure("http://api.example.com/dummies?sort=last-name", "The sort expression \"last-name\" does not begin with a direction indicator (+ or -).");
        }
    }
}
