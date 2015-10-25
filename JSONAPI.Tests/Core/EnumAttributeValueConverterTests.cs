using FluentAssertions;
using JSONAPI.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Core
{
    [TestClass]
    public class EnumAttributeValueConverterTests
    {
        public enum Int32Enum
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }

        public enum Int64Enum : long
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }

        private class Class1
        {
            public Int32Enum Value { get; set; }
        }

        private class Class2
        {
            public Int64Enum Value { get; set; }
        }

        [TestMethod]
        public void GetValue_for_int32_enum()
        {
            // Arrange
            var property = typeof (Class1).GetProperty("Value");
            var obj = new Class1
            {
                Value = Int32Enum.Value1
            };

            // Act
            var converter = new EnumAttributeValueConverter(property, typeof(Int32Enum), false);
            var actualValue = (JValue)converter.GetValue(obj);

            // Assert
            actualValue.Value.Should().Be((long)1);
        }

        [TestMethod]
        public void GetValue_for_int64_enum()
        {
            // Arrange
            var property = typeof(Class2).GetProperty("Value");
            var obj = new Class2
            {
                Value = Int64Enum.Value1
            };

            // Act
            var converter = new EnumAttributeValueConverter(property, typeof(Int64Enum), false);
            var actualValue = (JValue)converter.GetValue(obj);

            // Assert
            actualValue.Value.Should().Be((long)1);
        }
    }
}
