using FluentAssertions;
using JSONAPI.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Core
{
    [TestClass]
    public class PrimitiveTypeAttributeValueConverterTests
    {
        private class Class1
        {
            public int? NullableIntValue { get; set; }
        }

        [TestMethod]
        public void GetValue_for_null()
        {
            // Arrange
            var property = typeof (Class1).GetProperty("NullableIntValue");
            var obj = new Class1
            {
                NullableIntValue = null
            };

            // Act
            var converter = new PrimitiveTypeAttributeValueConverter<int?>(property);
            var actualValue = (JValue)converter.GetValue(obj);

            // Assert
            ((object)actualValue).Should().Be(null);
        }

        [TestMethod]
        public void SetValue_for_null()
        {
            // Arrange
            var property = typeof(Class1).GetProperty("NullableIntValue");
            var obj = new Class1
            {
                NullableIntValue = 4
            };

            // Act
            var converter = new PrimitiveTypeAttributeValueConverter<int?>(property);
            converter.SetValue(obj, JValue.CreateNull());

            // Assert
            obj.NullableIntValue.Should().Be(null);
        }
    }
}
