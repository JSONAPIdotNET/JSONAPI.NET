using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using FluentAssertions;
using JSONAPI.ActionFilters;
using JSONAPI.Core;
using JSONAPI.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.ActionFilters
{
    [TestClass]
    public class EnableSortingAttributeTests
    {
        private class Dummy
        {
            public string Id { get; set; }

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
                new Dummy { Id = "8", FirstName = "William", LastName = "Harrison" }
            };
            _fixturesQuery = _fixtures.AsQueryable();
        }

        private HttpActionExecutedContext CreateActionExecutedContext(IModelManager modelManager, string uri)
        {
            var formatter = new JsonApiFormatter(modelManager);

            var httpContent = new ObjectContent(typeof (IQueryable<Dummy>), _fixturesQuery, formatter);

            return new HttpActionExecutedContext
            {
                ActionContext = new HttpActionContext
                {
                    ControllerContext = new HttpControllerContext
                    {
                        Request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri))
                    }
                },
                Response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = httpContent
                }
            };
        }

        private HttpResponseMessage GetActionFilterResponse(string uri)
        {
            var modelManager = new ModelManager(new PluralizationService(new Dictionary<string, string>
            {
                { "Dummy", "Dummies" }
            }));
            modelManager.RegisterResourceType(typeof(Dummy));

            var filter = new EnableSortingAttribute(modelManager);

            var context = CreateActionExecutedContext(modelManager, uri);

            filter.OnActionExecuted(context);

            return context.Response;
        }

        private T[] GetArray<T>(string uri)
        {
            var response = GetActionFilterResponse(uri);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var returnedContent = (ObjectContent)response.Content;
            var returnedQueryable = (IQueryable<T>)returnedContent.Value;
            return returnedQueryable.ToArray();
        }

        private void Expect400(string uri, string expectedMessage)
        {
            Action action = () =>
            {
                GetActionFilterResponse(uri);
            };
            var response = action.ShouldThrow<HttpResponseException>().Which.Response;

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var value = (HttpError)((ObjectContent<HttpError>)response.Content).Value;
            value.Message.Should().Be(expectedMessage);
        }

        [TestMethod]
        public void Sorts_by_attribute_ascending()
        {
            var array = GetArray<Dummy>("http://api.example.com/dummies?sort=%2BfirstName");
            array.Should().BeInAscendingOrder(d => d.FirstName);
        }

        [TestMethod]
        public void Sorts_by_attribute_descending()
        {
            var array = GetArray<Dummy>("http://api.example.com/dummies?sort=-firstName");
            array.Should().BeInDescendingOrder(d => d.FirstName);
        }

        [TestMethod]
        public void Sorts_by_two_ascending_attributes()
        {
            var array = GetArray<Dummy>("http://api.example.com/dummies?sort=%2BlastName,%2BfirstName");
            array.Should().ContainInOrder(_fixtures.OrderBy(d => d.LastName + d.FirstName));
        }

        [TestMethod]
        public void Sorts_by_two_descending_attributes()
        {
            var array = GetArray<Dummy>("http://api.example.com/dummies?sort=-lastName,-firstName");
            array.Should().ContainInOrder(_fixtures.OrderByDescending(d => d.LastName + d.FirstName));
        }

        [TestMethod]
        public void Returns_400_if_sort_argument_is_empty()
        {
            Expect400("http://api.example.com/dummies?sort=", "The sort expression \"\" is invalid.");
        }

        [TestMethod]
        public void Returns_400_if_sort_argument_is_whitespace()
        {
            Expect400("http://api.example.com/dummies?sort= ", "The sort expression \"\" is invalid.");
        }

        [TestMethod]
        public void Returns_400_if_property_name_is_missing()
        {
            Expect400("http://api.example.com/dummies?sort=%2B", "The property name is missing.");
        }

        [TestMethod]
        public void Returns_400_if_property_name_is_whitespace()
        {
            Expect400("http://api.example.com/dummies?sort=%2B ", "The property name is missing.");
        }

        [TestMethod]
        public void Returns_400_if_no_property_exists()
        {
            Expect400("http://api.example.com/dummies?sort=%2Bfoobar", "The attribute \"foobar\" does not exist on type \"dummies\".");
        }

        [TestMethod]
        public void Returns_400_if_the_same_property_is_specified_more_than_once()
        {
            Expect400("http://api.example.com/dummies?sort=%2BlastName,%2BlastName", "The attribute \"lastName\" was specified more than once.");
        }

        [TestMethod]
        public void Returns_400_if_sort_argument_doesnt_start_with_plus_or_minus()
        {
            Expect400("http://api.example.com/dummies?sort=lastName", "The sort expression \"lastName\" does not begin with a direction indicator (+ or -).");
        }
    }
}
