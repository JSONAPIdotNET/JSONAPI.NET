using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FluentAssertions;
using JSONAPI.ActionFilters;
using JSONAPI.Core;
using JSONAPI.Documents.Builders;
using JSONAPI.QueryableTransformers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.ActionFilters
{
    [TestClass]
    public class DefaultPaginationTransformerTests
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

        private DefaultPaginationTransformer GetTransformer(int maxPageSize)
        {
            return new DefaultPaginationTransformer(maxPageSize);
        }
            
        private Dummy[] GetArray(string uri, int maxPageSize = 50)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return GetTransformer(maxPageSize).ApplyPagination(_fixturesQuery, request).PagedQuery.ToArray();
        }

        [TestMethod]
        public void ApplyPagination_has_no_effect_when_no_paging_parameters_are_supplied()
        {
            var array = GetArray("http://api.example.com/dummies");
            array.Length.Should().Be(9);
        }

        [TestMethod]
        public void ApplyPagination_returns_all_results_when_they_are_within_page()
        {
            var array = GetArray("http://api.example.com/dummies?page[number]=0&page[size]=10");
            array.Length.Should().Be(9);
        }

        [TestMethod]
        public void ApplyPagination_returns_first_page_of_data()
        {
            var array = GetArray("http://api.example.com/dummies?page[number]=0&page[size]=4");
            array.Should().BeEquivalentTo(_fixtures[0], _fixtures[1], _fixtures[2], _fixtures[3]);
        }

        [TestMethod]
        public void ApplyPagination_returns_second_page_of_data()
        {
            var array = GetArray("http://api.example.com/dummies?page[number]=1&page[size]=4");
            array.Should().BeEquivalentTo(_fixtures[4], _fixtures[5], _fixtures[6], _fixtures[7]);
        }

        [TestMethod]
        public void ApplyPagination_returns_page_at_end()
        {
            var array = GetArray("http://api.example.com/dummies?page[number]=2&page[size]=4");
            array.Should().BeEquivalentTo(_fixtures[8]);
        }

        [TestMethod]
        public void ApplyPagination_returns_nothing_for_page_after_end()
        {
            var array = GetArray("http://api.example.com/dummies?page[number]=3&page[size]=4");
            array.Length.Should().Be(0);
        }

        [TestMethod]
        public void ApplyPagination_uses_max_page_size_when_requested_page_size_is_higher()
        {
            var array = GetArray("http://api.example.com/dummies?page[number]=1&page[size]=8", 3);
            array.Should().BeEquivalentTo(_fixtures[3], _fixtures[4], _fixtures[5]);
        }

        [TestMethod]
        public void ApplyPagination_throws_exception_if_page_number_is_negative()
        {
            Action action = () =>
            {
                GetArray("http://api.example.com/dummies?page[number]=-4&page[size]=4");
            };
            action.ShouldThrow<JsonApiException>().And.Error.Detail.Should().Be("Page number must not be negative.");
        }

        [TestMethod]
        public void ApplyPagination_throws_exception_if_page_size_is_negative()
        {
            Action action = () =>
            {
                GetArray("http://api.example.com/dummies?page[number]=0&page[size]=-4");
            };
            action.ShouldThrow<JsonApiException>().And.Error.Detail.Should().Be("Page size must be greater than or equal to 1.");
        }

        [TestMethod]
        public void ApplyPagination_throws_exception_when_page_size_is_zero()
        {
            Action action = () =>
            {
                GetArray("http://api.example.com/dummies?page[number]=0&page[size]=0");
            };
            action.ShouldThrow<JsonApiException>().And.Error.Detail.Should().Be("Page size must be greater than or equal to 1.");
        }

        [TestMethod]
        public void ApplyPagination_throws_exception_if_page_number_specified_but_not_size()
        {
            Action action = () =>
            {
                GetArray("http://api.example.com/dummies?page[number]=0");
            };
            action.ShouldThrow<JsonApiException>().And.Error.Detail.Should().Be("In order for paging to work properly, if either page.number or page.size is set, both must be.");
        }

        [TestMethod]
        public void ApplyPagination_throws_exception_if_page_size_specified_but_not_number()
        {
            Action action = () =>
            {
                GetArray("http://api.example.com/dummies?page[size]=0");
            };
            action.ShouldThrow<JsonApiException>().And.Error.Detail.Should().Be("In order for paging to work properly, if either page.number or page.size is set, both must be.");
        }

        [TestMethod]
        public void DefaultPaginationTransformer_cannot_be_instantiated_if_max_page_size_is_zero()
        {
            Action action = () =>
            {
                GetTransformer(0);
            };
            action.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void DefaultPaginationTransformer_cannot_be_instantiated_if_max_page_size_is_negative()
        {
            Action action = () =>
            {
                GetTransformer(-4);
            };
            action.ShouldThrow<ArgumentOutOfRangeException>();
        }
    }
}
