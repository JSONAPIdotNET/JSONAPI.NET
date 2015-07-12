using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using Effort;
using Effort.DataLoaders;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    internal static class TestHelpers
    {
        // http://stackoverflow.com/questions/21175713/no-entity-framework-provider-found-for-the-ado-net-provider-with-invariant-name
        // ReSharper disable once NotAccessedField.Local
        private static volatile Type _dependency;
        static TestHelpers()
        {
            _dependency = typeof(System.Data.Entity.SqlServer.SqlProviderServices);
        }

        public static string ReadEmbeddedFile(string path)
        {
            var resourcePath = "JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests." + path.Replace("\\", ".").Replace("/", ".");
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
            {
                if (stream == null) throw new Exception("Could not find a file at the path: " + path);
                return new StreamReader(stream).ReadToEnd();
            }
        }

        public static DbConnection GetEffortConnection(string relativeDataPath)
        {
            var dataPath = Path.GetFullPath(relativeDataPath);
            var dataLoader = new CsvDataLoader(dataPath);
            return DbConnectionFactory.CreateTransient(dataLoader);
        }
    }
}
