using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using JSONAPI.Json;
using Moq;

namespace JSONAPI.Tests
{
    internal static class TestHelpers
    {
        public static string ReadEmbeddedFile(string path)
        {
            var resourcePath = "JSONAPI.Tests." + path.Replace("\\", ".").Replace("/", ".");
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
            {
                if (stream == null) throw new Exception("Could not find a file at the path: " + path);
                return new StreamReader(stream).ReadToEnd();
            }
        }

        public static void StreamContentsMatchFixtureContents(MemoryStream stream, string fixtureFileName)
        {
            var output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            var expectedJson = ReadEmbeddedFile(fixtureFileName);
            var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
            output.Should().Be(minifiedExpectedJson);
        }

        public static void SetupIQueryable<T>(this Mock<T> mock, IQueryable queryable)
            where T : class, IQueryable
        {
            mock.Setup(r => r.GetEnumerator()).Returns(queryable.GetEnumerator());
            mock.Setup(r => r.Provider).Returns(queryable.Provider);
            mock.Setup(r => r.ElementType).Returns(queryable.ElementType);
            mock.Setup(r => r.Expression).Returns(queryable.Expression);
        }
    }
}
