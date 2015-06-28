using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Json;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Json
{
    public abstract class JsonApiSerializerTestsBase
    {
        protected async Task AssertSerializeOutput<TSerializer, TComponent>(TSerializer serializer, TComponent component, string expectedJsonFile)
            where TSerializer : IJsonApiSerializer<TComponent>
        {
            var output = await GetSerializedString(serializer, component);

            // Assert
            var expectedJson = TestHelpers.ReadEmbeddedFile(expectedJsonFile);
            var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
            output.Should().Be(minifiedExpectedJson);
        }

        protected async Task<string> GetSerializedString<TSerializer, TComponent>(TSerializer serializer, TComponent component)
            where TSerializer : IJsonApiSerializer<TComponent>
        {
            using (var stream = new MemoryStream())
            {
                using (var textWriter = new StreamWriter(stream))
                {
                    using (var writer = new JsonTextWriter(textWriter))
                    {
                        await serializer.Serialize(component, writer);
                        writer.Flush();
                        return Encoding.ASCII.GetString(stream.ToArray());
                    }
                }
            }
        }

        protected async Task<TComponent> GetDeserializedOutput<TSerializer, TComponent>(TSerializer serializer, string requestFileName)
            where TSerializer : IJsonApiSerializer<TComponent>
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
                        var deserialized = await serializer.Deserialize(reader, "");
                        reader.Read().Should().BeFalse(); // There should be nothing left to read.
                        return deserialized;
                    }
                }
            }
        }
    }
}
