﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using FluentAssertions;
using JSONAPI.EntityFramework.ActionFilters;
using JSONAPI.EntityFramework.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.EntityFramework.Tests.ActionFilters
{
    [TestClass]
    public class EnumerateQueryableAsyncAttributeTests
    {
        public class Dummy
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }

        private IQueryable<Dummy> _fixtures;

        [TestInitialize]
        public void SetupFixtures()
        {
            _fixtures = new List<Dummy>()
            {
                new Dummy
                {
                    Id = "1",
                    Name = "Blue"
                },
                new Dummy
                {
                    Id = "2",
                    Name = "Red"
                },
                new Dummy
                {
                    Id = "3",
                    Name = "Green"
                }
            }.AsQueryable();
        }

        private HttpActionExecutedContext CreateActionExecutedContext(IDbAsyncEnumerator<Dummy> asyncEnumerator)
        {
            var mockSet = new Mock<DbSet<Dummy>>();
            mockSet.As<IDbAsyncEnumerable<Dummy>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(asyncEnumerator);

            mockSet.As<IQueryable<Dummy>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<Dummy>(_fixtures.Provider));

            mockSet.As<IQueryable<Dummy>>().Setup(m => m.Expression).Returns(_fixtures.Expression);
            mockSet.As<IQueryable<Dummy>>().Setup(m => m.ElementType).Returns(_fixtures.ElementType);
            mockSet.As<IQueryable<Dummy>>().Setup(m => m.GetEnumerator()).Returns(_fixtures.GetEnumerator());

            var formatter = new JsonMediaTypeFormatter();

            var httpContent = new ObjectContent(typeof(IQueryable<Dummy>), mockSet.Object, formatter);

            return new HttpActionExecutedContext
            {
                ActionContext = new HttpActionContext
                {
                    ControllerContext = new HttpControllerContext
                    {
                        Request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/dummies")
                    }
                },
                Response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = httpContent
                }
            };
        }

        [TestMethod]
        public async Task ResolvesQueryable()
        {
            var actionFilter = new EnumerateQueryableAsyncAttribute();

            var context = CreateActionExecutedContext(new TestDbAsyncEnumerator<Dummy>(_fixtures.GetEnumerator()));

            await actionFilter.OnActionExecutedAsync(context, new CancellationToken());

            var objectContent = context.Response.Content as ObjectContent;
            objectContent.Should().NotBeNull();

            var array = objectContent.Value as Dummy[];
            array.Should().NotBeNull();
            array.Length.Should().Be(3);
            array[0].Id.Should().Be("1");
            array[1].Id.Should().Be("2");
            array[2].Id.Should().Be("3");
        }

        [TestMethod]
        public void CancelsProperly()
        {
            var actionFilter = new EnumerateQueryableAsyncAttribute();

            var context = CreateActionExecutedContext(new WaitsUntilCancellationDbAsyncEnumerator<Dummy>(1000, _fixtures.GetEnumerator()));

            var cts = new CancellationTokenSource();
            cts.CancelAfter(300);

            Func<Task> action = async () =>
            {
                await actionFilter.OnActionExecutedAsync(context, cts.Token);
            };
            action.ShouldThrow<TaskCanceledException>();
        }
    }
}
