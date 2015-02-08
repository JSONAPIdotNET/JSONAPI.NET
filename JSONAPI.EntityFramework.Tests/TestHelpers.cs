using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using Effort;
using Effort.DataLoaders;

namespace JSONAPI.EntityFramework.Tests
{
    internal static class TestHelpers
    {
        public static string ReadEmbeddedFile(string path)
        {
            var resourcePath = "JSONAPI.EntityFramework.Tests." + path.Replace("\\", ".").Replace("/", ".");
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
