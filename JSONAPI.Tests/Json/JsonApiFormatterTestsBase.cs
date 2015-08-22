using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Json;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Json
{
    public abstract class JsonApiFormatterTestsBase
    {
        protected async Task AssertSerializeOutput<TFormatter, TComponent>(TFormatter formatter, TComponent component, string expectedJsonFile)
            where TFormatter : IJsonApiFormatter<TComponent>
        {
            var output = await GetSerializedString(formatter, component);

            // Assert
            var expectedJson = TestHelpers.ReadEmbeddedFile(expectedJsonFile);
            var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
            output.Should().Be(minifiedExpectedJson);
        }

        protected async Task<string> GetSerializedString<TFormatter, TComponent>(TFormatter formatter, TComponent component)
            where TFormatter : IJsonApiFormatter<TComponent>
        {
            using (var stream = new MemoryStream())
            {
                using (var textWriter = new StreamWriter(stream))
                {
                    using (var writer = new JsonTextWriter(textWriter))
                    {
                        await formatter.Serialize(component, writer);
                        writer.Flush();
                        return Encoding.ASCII.GetString(stream.ToArray());
                    }
                }
            }
        }

        protected async Task<TComponent> GetDeserializedOutput<TFormatter, TComponent>(TFormatter formatter, string requestFileName)
            where TFormatter : IJsonApiFormatter<TComponent>
        {
            var resourcePath = "JSONAPI.Tests." + requestFileName.Replace("\\", ".").Replace("/", ".");
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                using (var textReader = new StreamReader(resourceStream))
                {
                    using (var reader = new JsonTextReader(textReader))
                    {
                        reader.Read();
                        var deserialized = await formatter.Deserialize(reader, "");
                        reader.Read().Should().BeFalse(); // There should be nothing left to read.
                        return deserialized;
                    }
                }
            }
        }
    }
}
