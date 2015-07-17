using System;
using JSONAPI.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSONAPI.Core;
using JSONAPI.Tests.Models;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Core
{
    [TestClass]
    public class ResourceTypeRegistrarTests
    {
        private class InvalidModel // No Id discernable!
        {
            public string Data { get; set; }
        }

        private class CustomIdModel
        {
            [UseAsId]
            public Guid Uuid { get; set; }

            public string Data { get; set; }
        }

        private class Salad
        {
            public string Id { get; set; }

            [JsonProperty("salad-type")]
            public string TheSaladType { get; set; }

            [JsonProperty("salad-type")]
            public string AnotherSaladType { get; set; }
        }

        private class Continent
        {
            [UseAsId]
            public string Name { get; set; }

            public string Id { get; set; }
        }

        private class Boat
        {
            public string Id { get; set; }

            public string Type { get; set; }
        }

        [TestMethod]
        public void Cant_register_type_with_missing_id()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            Action action = () => registrar.BuildRegistration(typeof(InvalidModel));

            // Assert
            action.ShouldThrow<InvalidOperationException>()
                .Which.Message.Should()
                .Be("Unable to determine Id property for type `InvalidModel`.");
        }

        [TestMethod]
        public void Cant_register_type_with_non_id_property_called_id()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            Action action = () => registrar.BuildRegistration(typeof(Continent));

            // Assert
            action.ShouldThrow<InvalidOperationException>()
                .Which.Message.Should()
                .Be("Failed to register type `Continent` because it contains a non-id property that would serialize as \"id\".");
        }

        [TestMethod]
        public void Cant_register_type_with_property_called_type()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            Action action = () => registrar.BuildRegistration(typeof(Boat));

            // Assert
            action.ShouldThrow<InvalidOperationException>()
                .Which.Message.Should()
                .Be("Failed to register type `Boat` because it contains a property that would serialize as \"type\".");
        }

        [TestMethod]
        public void Cant_register_type_with_two_properties_with_the_same_name()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));
            Type saladType = typeof(Salad);

            // Act
            Action action = () => registrar.BuildRegistration(saladType);

            // Assert
            action.ShouldThrow<InvalidOperationException>().Which.Message.Should()
                .Be("Failed to register type `Salad` because contains multiple properties that would serialize as `salad-type`.");
        }

        [TestMethod]
        public void BuildRegistration_sets_up_registration_correctly()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var postReg = registrar.BuildRegistration(typeof(Post));
            
            // Assert
            postReg.IdProperty.Should().BeSameAs(typeof(Post).GetProperty("Id"));
            postReg.ResourceTypeName.Should().Be("posts");
            postReg.Attributes.Length.Should().Be(1);
            postReg.Attributes.First().Property.Should().BeSameAs(typeof(Post).GetProperty("Title"));
            postReg.Relationships.Length.Should().Be(2);
            postReg.Relationships[0].IsToMany.Should().BeFalse();
            postReg.Relationships[0].Property.Should().BeSameAs(typeof(Post).GetProperty("Author"));
            postReg.Relationships[0].SelfLinkTemplate.Should().BeNull();
            postReg.Relationships[0].RelatedResourceLinkTemplate.Should().BeNull();
            postReg.Relationships[1].IsToMany.Should().BeTrue();
            postReg.Relationships[1].Property.Should().BeSameAs(typeof(Post).GetProperty("Comments"));
            postReg.Relationships[1].SelfLinkTemplate.Should().Be("/posts/{1}/relationships/comments");
            postReg.Relationships[1].RelatedResourceLinkTemplate.Should().Be("/posts/{1}/comments");
        }

        private AttributeGrabBag InitializeGrabBag()
        {
            return new AttributeGrabBag()
            {
                Id = "2",
                BooleanField = true,
                NullableBooleanField = true,
                SbyteField = 123,
                NullableSbyteField = 123,
                ByteField = 253,
                NullableByteField = 253,
                Int16Field = 32000,
                NullableInt16Field = 32000,
                Uint16Field = 64000,
                NullableUint16Field = 64000,
                Int32Field = 2000000000,
                NullableInt32Field = 2000000000,
                Uint32Field = 3000000000,
                NullableUint32Field = 3000000000,
                Int64Field = 9223372036854775807,
                NullableInt64Field = 9223372036854775807,
                Uint64Field = 9223372036854775808,
                NullableUint64Field = 9223372036854775808,
                DoubleField = 1056789.123,
                NullableDoubleField = 1056789.123,
                SingleField = 1056789.123f,
                NullableSingleField = 1056789.123f,
                DecimalField = 1056789.123m,
                NullableDecimalField = 1056789.123m,
                DateTimeField = new DateTime(1776, 07, 04),
                NullableDateTimeField = new DateTime(1776, 07, 04),
                DateTimeOffsetField = new DateTimeOffset(new DateTime(1776, 07, 04), new TimeSpan(-5, 0, 0)),
                NullableDateTimeOffsetField = new DateTimeOffset(new DateTime(1776, 07, 04), new TimeSpan(-5, 0, 0)),
                GuidField = new Guid("6566F9B4-5245-40DE-890D-98B40A4AD656"),
                NullableGuidField = new Guid("3D1FB81E-43EE-4D04-AF91-C8A326341293"),
                StringField = "Some string 156",
                EnumField = SampleEnum.Value1,
                NullableEnumField = SampleEnum.Value2,
                ComplexAttributeField = "{\"foo\": { \"baz\": [11] }, \"bar\": 5}"
            };
        }

        private void AssertAttribute<TPropertyType, TTokenType>(IResourceTypeRegistration reg, string attributeName,
            JToken tokenToSet, TPropertyType expectedPropertyValue, TTokenType expectedTokenAfterSet, Func<AttributeGrabBag, TPropertyType> getPropertyFunc)
        {
            var grabBag = InitializeGrabBag();

            var field = reg.GetFieldByName(attributeName);
            var attribute = (ResourceTypeAttribute) field;
            attribute.JsonKey.Should().Be(attributeName);

            attribute.SetValue(grabBag, tokenToSet);
            var propertyValueAfterSet = getPropertyFunc(grabBag);
            propertyValueAfterSet.Should().Be(expectedPropertyValue);
            
            var convertedToken = attribute.GetValue(grabBag);
            if (expectedTokenAfterSet == null)
                convertedToken.Should().BeNull();
            else
            {
                var convertedTokenValue = convertedToken.Value<TTokenType>();
                convertedTokenValue.Should().Be(expectedTokenAfterSet);
            }
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_boolean_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "boolean-field", false, false, false, g => g.BooleanField);
            AssertAttribute(reg, "boolean-field", true, true, true, g => g.BooleanField);
            AssertAttribute(reg, "boolean-field", null, false, false, g => g.BooleanField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_boolean_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-boolean-field", false, false, false, g => g.NullableBooleanField);
            AssertAttribute(reg, "nullable-boolean-field", true, true, true, g => g.NullableBooleanField);
            AssertAttribute(reg, "nullable-boolean-field", null, null, (Boolean?) null, g => g.NullableBooleanField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_SByte_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "sbyte-field", 0, 0, 0, g => g.SbyteField);
            AssertAttribute(reg, "sbyte-field", 12, 12, 12, g => g.SbyteField);
            AssertAttribute(reg, "sbyte-field", -12, -12, -12, g => g.SbyteField);
            AssertAttribute(reg, "sbyte-field", null, 0, 0, g => g.SbyteField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_SByte_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-sbyte-field", 0, (SByte?)0, (SByte?)0, g => g.NullableSbyteField);
            AssertAttribute(reg, "nullable-sbyte-field", 12, (SByte?)12, (SByte?)12, g => g.NullableSbyteField);
            AssertAttribute(reg, "nullable-sbyte-field", -12, (SByte?)-12, (SByte?)-12, g => g.NullableSbyteField);
            AssertAttribute(reg, "nullable-sbyte-field", null, null, (SByte?)null, g => g.NullableSbyteField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_Byte_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "byte-field", 0, 0, 0, g => g.ByteField);
            AssertAttribute(reg, "byte-field", 12, 12, 12, g => g.ByteField);
            AssertAttribute(reg, "byte-field", null, 0, 0, g => g.ByteField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_Byte_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-byte-field", 0, (Byte?)0, (Byte?)0, g => g.NullableByteField);
            AssertAttribute(reg, "nullable-byte-field", 12, (Byte?)12, (Byte?)12, g => g.NullableByteField);
            AssertAttribute(reg, "nullable-byte-field", null, null, (Byte?)null, g => g.NullableByteField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_Int16_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "int16-field", 0, 0, 0, g => g.Int16Field);
            AssertAttribute(reg, "int16-field", 4000, 4000, 4000, g => g.Int16Field);
            AssertAttribute(reg, "int16-field", -4000, -4000, -4000, g => g.Int16Field);
            AssertAttribute(reg, "int16-field", null, 0, 0, g => g.Int16Field);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_Int16_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-int16-field", 0, (Int16?)0, (Int16?)0, g => g.NullableInt16Field);
            AssertAttribute(reg, "nullable-int16-field", 4000, (Int16?)4000, (Int16?)4000, g => g.NullableInt16Field);
            AssertAttribute(reg, "nullable-int16-field", -4000, (Int16?)-4000, (Int16?)-4000, g => g.NullableInt16Field);
            AssertAttribute(reg, "nullable-int16-field", null, null, (Int16?)null, g => g.NullableInt16Field);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_UInt16_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "uint16-field", 0, 0, 0, g => g.Uint16Field);
            AssertAttribute(reg, "uint16-field", 4000, 4000, 4000, g => g.Uint16Field);
            AssertAttribute(reg, "uint16-field", null, 0, 0, g => g.Uint16Field);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_UInt16_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-uint16-field", 0, (UInt16?)0, (UInt16?)0, g => g.NullableUint16Field);
            AssertAttribute(reg, "nullable-uint16-field", 4000, (UInt16?)4000, (UInt16?)4000, g => g.NullableUint16Field);
            AssertAttribute(reg, "nullable-uint16-field", null, null, (UInt16?)null, g => g.NullableUint16Field);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_Int32_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "int32-field", 0, 0, 0, g => g.Int32Field);
            AssertAttribute(reg, "int32-field", 2000000, 2000000, 2000000, g => g.Int32Field);
            AssertAttribute(reg, "int32-field", -2000000, -2000000, -2000000, g => g.Int32Field);
            AssertAttribute(reg, "int32-field", null, 0, 0, g => g.Int32Field);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_Int32_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-int32-field", 0, 0, (Int32?)0, g => g.NullableInt32Field);
            AssertAttribute(reg, "nullable-int32-field", 2000000, 2000000, (Int32?)2000000, g => g.NullableInt32Field);
            AssertAttribute(reg, "nullable-int32-field", -2000000, -2000000, (Int32?)-2000000, g => g.NullableInt32Field);
            AssertAttribute(reg, "nullable-int32-field", null, null, (Int32?)null, g => g.NullableInt32Field);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_UInt32_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "uint32-field", 0, (UInt32)0, (UInt32)0, g => g.Uint32Field);
            AssertAttribute(reg, "uint32-field", 2000000, (UInt32)2000000, (UInt32)2000000, g => g.Uint32Field);
            AssertAttribute(reg, "uint32-field", null, (UInt32)0, (UInt32)0, g => g.Uint32Field);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_UInt32_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-uint32-field", 0, (UInt32?)0, (UInt32?)0, g => g.NullableUint32Field);
            AssertAttribute(reg, "nullable-uint32-field", 2000000, (UInt32?)2000000, (UInt32?)2000000, g => g.NullableUint32Field);
            AssertAttribute(reg, "nullable-uint32-field", null, null, (UInt32?)null, g => g.NullableUint32Field);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_Int64_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "int64-field", 0, 0, 0, g => g.Int64Field);
            AssertAttribute(reg, "int64-field", 20000000000, 20000000000, 20000000000, g => g.Int64Field);
            AssertAttribute(reg, "int64-field", -20000000000, -20000000000, -20000000000, g => g.Int64Field);
            AssertAttribute(reg, "int64-field", null, 0, 0, g => g.Int64Field);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_Int64_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-int64-field", 0, 0, (Int64?)0, g => g.NullableInt64Field);
            AssertAttribute(reg, "nullable-int64-field", 20000000000, 20000000000, (Int64?)20000000000, g => g.NullableInt64Field);
            AssertAttribute(reg, "nullable-int64-field", -20000000000, -20000000000, (Int64?)-20000000000, g => g.NullableInt64Field);
            AssertAttribute(reg, "nullable-int64-field", null, null, (Int64?)null, g => g.NullableInt64Field);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_UInt64_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "uint64-field", 0, (UInt64)0, (UInt64)0, g => g.Uint64Field);
            AssertAttribute(reg, "uint64-field", 20000000000, (UInt64)20000000000, (UInt64)20000000000, g => g.Uint64Field);
            AssertAttribute(reg, "uint64-field", null, (UInt64)0, (UInt64)0, g => g.Uint64Field);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_UInt64_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-uint64-field", 0, (UInt64?)0, (UInt64?)0, g => g.NullableUint64Field);
            AssertAttribute(reg, "nullable-uint64-field", 20000000000, (UInt64?)20000000000, (UInt64?)20000000000, g => g.NullableUint64Field);
            AssertAttribute(reg, "nullable-uint64-field", null, null, (UInt64?)null, g => g.NullableUint64Field);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_Single_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "single-field", 0f, 0f, 0f, g => g.SingleField);
            AssertAttribute(reg, "single-field", 20000000000.1234f, 20000000000.1234f, 20000000000.1234f, g => g.SingleField);
            AssertAttribute(reg, "single-field", -20000000000.1234f, -20000000000.1234f, -20000000000.1234f, g => g.SingleField);
            AssertAttribute(reg, "single-field", null, 0, 0, g => g.SingleField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_Single_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-single-field", 0f, 0f, 0f, g => g.NullableSingleField);
            AssertAttribute(reg, "nullable-single-field", 20000000000.1234f, 20000000000.1234f, (Int64?)20000000000.1234f, g => g.NullableSingleField);
            AssertAttribute(reg, "nullable-single-field", -20000000000.1234f, -20000000000.1234f, -20000000000.1234f, g => g.NullableSingleField);
            AssertAttribute(reg, "nullable-single-field", null, null, (Single?)null, g => g.NullableSingleField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_Double_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "double-field", 0d, 0d, 0d, g => g.DoubleField);
            AssertAttribute(reg, "double-field", 20000000000.1234d, 20000000000.1234d, 20000000000.1234d, g => g.DoubleField);
            AssertAttribute(reg, "double-field", null, 0d, 0d, g => g.DoubleField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_Double_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-double-field", 0d, 0d, 0d, g => g.NullableDoubleField);
            AssertAttribute(reg, "nullable-double-field", 20000000000.1234d, 20000000000.1234d, 20000000000.1234d, g => g.NullableDoubleField);
            AssertAttribute(reg, "nullable-double-field", null, null, (Double?)null, g => g.NullableDoubleField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_Decimal_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "decimal-field", "0", 0m, "0", g => g.DecimalField);
            AssertAttribute(reg, "decimal-field", "20000000000.1234", 20000000000.1234m, "20000000000.1234", g => g.DecimalField);
            AssertAttribute(reg, "decimal-field", null, 0m, "0", g => g.DecimalField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_Decimal_field_non_en_US()
        {
            // Set up non US culture
            var culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("se-SE");

            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            AssertAttribute(reg, "decimal-field", "20000000000.1234", 20000000000.1234m, "20000000000.1234", g => g.DecimalField);

            Thread.CurrentThread.CurrentCulture = culture;
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_Decimal_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-decimal-field", "0", 0m, "0", g => g.NullableDecimalField);
            AssertAttribute(reg, "nullable-decimal-field", "20000000000.1234", 20000000000.1234m, "20000000000.1234", g => g.NullableDecimalField);
            AssertAttribute(reg, "nullable-decimal-field", null, null, (string)null, g => g.NullableDecimalField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_guid_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            var guid = new Guid("6566f9b4-5245-40de-890d-98b40a4ad656");
            AssertAttribute(reg, "guid-field", "6566f9b4-5245-40de-890d-98b40a4ad656", guid, "6566f9b4-5245-40de-890d-98b40a4ad656", g => g.GuidField);
            AssertAttribute(reg, "guid-field", null, new Guid(), "00000000-0000-0000-0000-000000000000", g => g.GuidField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_guid_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            var guid = new Guid("6566f9b4-5245-40de-890d-98b40a4ad656");
            AssertAttribute(reg, "nullable-guid-field", "6566f9b4-5245-40de-890d-98b40a4ad656", guid, "6566f9b4-5245-40de-890d-98b40a4ad656", g => g.NullableGuidField);
            AssertAttribute(reg, "nullable-guid-field", null, null, (Guid?)null, g => g.NullableGuidField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_DateTime_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "date-time-field", "1776-07-04", new DateTime(1776, 07, 04), "1776-07-04T00:00:00", g => g.DateTimeField);
            AssertAttribute(reg, "date-time-field", "1776-07-04T00:00:00", new DateTime(1776, 07, 04), "1776-07-04T00:00:00", g => g.DateTimeField);
            AssertAttribute(reg, "date-time-field", null, new DateTime(), "0001-01-01T00:00:00", g => g.DateTimeField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_DateTime_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-date-time-field", "1776-07-04", new DateTime(1776, 07, 04), "1776-07-04T00:00:00", g => g.NullableDateTimeField);
            AssertAttribute(reg, "nullable-date-time-field", "1776-07-04T00:00:00", new DateTime(1776, 07, 04), "1776-07-04T00:00:00", g => g.NullableDateTimeField);
            AssertAttribute(reg, "nullable-date-time-field", null, null, (DateTime?)null, g => g.NullableDateTimeField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_DateTimeOffset_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            var testDateTimeOffset1 = new DateTimeOffset(new DateTime(1776, 07, 04), TimeSpan.FromHours(-5));
            var testDateTimeOffset2 = new DateTimeOffset(new DateTime(1776, 07, 04, 12, 30, 0), TimeSpan.FromHours(0));
            var testDateTimeOffset3 = new DateTimeOffset(new DateTime(2015, 03, 11, 04, 31, 0), TimeSpan.FromHours(0));
            AssertAttribute(reg, "date-time-offset-field", "1776-07-04T00:00:00-05:00", testDateTimeOffset1, "1776-07-04T00:00:00.0000000-05:00", g => g.DateTimeOffsetField);
            AssertAttribute(reg, "date-time-offset-field", "1776-07-04T12:30:00+00:00", testDateTimeOffset2, "1776-07-04T12:30:00.0000000+00:00", g => g.DateTimeOffsetField);
            AssertAttribute(reg, "date-time-offset-field", "2015-03-11T04:31:00.0000000+00:00", testDateTimeOffset3, "2015-03-11T04:31:00.0000000+00:00", g => g.DateTimeOffsetField);
            AssertAttribute(reg, "date-time-offset-field", null, new DateTimeOffset(), "0001-01-01T00:00:00.0000000+00:00", g => g.DateTimeOffsetField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_DateTimeOffset_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            var testDateTimeOffset = new DateTimeOffset(new DateTime(1776, 07, 04), TimeSpan.FromHours(-5));
            AssertAttribute(reg, "nullable-date-time-offset-field", "1776-07-04T00:00:00-05:00", testDateTimeOffset, "1776-07-04T00:00:00.0000000-05:00",
                g => g.NullableDateTimeOffsetField);
            AssertAttribute(reg, "nullable-date-time-offset-field", null, null, (DateTimeOffset?)null,
                g => g.NullableDateTimeOffsetField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_string_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "string-field", "asdf", "asdf", "asdf", g => g.StringField);
            AssertAttribute(reg, "string-field", null, null, (string)null, g => g.StringField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_enum_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "enum-field", (int)SampleEnum.Value1, SampleEnum.Value1, (int)SampleEnum.Value1, g => g.EnumField);
            AssertAttribute(reg, "enum-field", null, (SampleEnum)0, 0, g => g.EnumField);
        }

        [TestMethod]
        public void BuildRegistration_sets_up_correct_attribute_for_nullable_enum_field()
        {
            // Arrange
            var registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var reg = registrar.BuildRegistration(typeof(AttributeGrabBag));

            // Assert
            AssertAttribute(reg, "nullable-enum-field", (int)SampleEnum.Value1, SampleEnum.Value1, (int)SampleEnum.Value1, g => g.NullableEnumField);
            AssertAttribute(reg, "nullable-enum-field", null, null, (SampleEnum?)null, g => g.NullableEnumField);
        }
    }
}
