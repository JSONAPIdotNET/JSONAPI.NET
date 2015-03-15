using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FluentAssertions;
using JSONAPI.ActionFilters;
using JSONAPI.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.ActionFilters
{
    [TestClass]
    public class DefaultFilteringTransformerTests : QueryableTransformerTestsBase
    {
        private enum SomeEnum
        {
            EnumValue1 = 1,
            EnumValue2 = 2,
            EnumValue3 = 3
        }

        private class SomeUnknownType
        {

        }

        private class RelatedItemWithId
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        private class Dummy
        {
            public string Id { get; set; }
            public string StringField { get; set; }
            public DateTime DateTimeField { get; set; }
            public DateTime? NullableDateTimeField { get; set; }
            public DateTimeOffset DateTimeOffsetField { get; set; }
            public DateTimeOffset? NullableDateTimeOffsetField { get; set; }
            public SomeEnum EnumField { get; set; }
            public SomeEnum? NullableEnumField { get; set; }
            public Decimal DecimalField { get; set; }
            public Decimal? NullableDecimalField { get; set; }
            public Boolean BooleanField { get; set; }
            public Boolean? NullableBooleanField { get; set; }
            public SByte SByteField { get; set; }
            public SByte? NullableSByteField { get; set; }
            public Byte ByteField { get; set; }
            public Byte? NullableByteField { get; set; }
            public Int16 Int16Field { get; set; }
            public Int16? NullableInt16Field { get; set; }
            public UInt16 UInt16Field { get; set; }
            public UInt16? NullableUInt16Field { get; set; }
            public Int32 Int32Field { get; set; }
            public Int32? NullableInt32Field { get; set; }
            public UInt32 UInt32Field { get; set; }
            public UInt32? NullableUInt32Field { get; set; }
            public Int64 Int64Field { get; set; }
            public Int64? NullableInt64Field { get; set; }
            public UInt64 UInt64Field { get; set; }
            public UInt64? NullableUInt64Field { get; set; }
            public Double DoubleField { get; set; }
            public Double? NullableDoubleField { get; set; }
            public Single SingleField { get; set; }
            public Single? NullableSingleField { get; set; }
            public SomeUnknownType UnknownTypeField { get; set; }
            public RelatedItemWithId ToOneRelatedItem { get; set; }
            public ICollection<RelatedItemWithId> ToManyRelatedItems { get; set; }
        }

        private IList<Dummy> _fixtures;
        private IQueryable<Dummy> _fixturesQuery;
            
        [TestInitialize]
        public void SetupFixtures()
        {
            _fixtures = new List<Dummy>
            {
                #region StringField

                new Dummy
                {
                    Id = "100",
                    StringField = "String value 1"
                },
                new Dummy
                {
                    Id = "101",
                    StringField = "String value 2"
                },
                new Dummy
                {
                    Id = "102",
                    StringField = "String value 2",
                    EnumField = SomeEnum.EnumValue3
                },

                #endregion

                #region DateTimeField

                new Dummy
                {
                    Id = "110",
                    DateTimeField = new DateTime(1930, 11, 7)
                },
                new Dummy
                {
                    Id = "111",
                    DateTimeField = new DateTime(1980, 6, 2)
                },

                #endregion

                #region NullableDateTimeField
		
                new Dummy
                {
                    Id = "120",
                    NullableDateTimeField = new DateTime(1961, 2, 18)
                },

                #endregion

                #region DateTimeOffsetField

                new Dummy
                {
                    Id = "130",
                    DateTimeOffsetField = new DateTime(1991, 1, 3)
                },
                new Dummy
                {
                    Id = "131",
                    DateTimeOffsetField = new DateTime(2004, 10, 31)
                },

                #endregion
                
                #region NullableDateTimeOffsetField

                new Dummy
                {
                    Id = "140",
                    NullableDateTimeOffsetField = new DateTime(2014, 5, 5)
                },

                #endregion

                #region EnumField

                new Dummy
                {
                    Id = "150",
                    EnumField = SomeEnum.EnumValue1
                },
                new Dummy
                {
                    Id = "151",
                    EnumField = SomeEnum.EnumValue2
                },

                #endregion

                #region NullableEnumField

                new Dummy
                {
                    Id = "160",
                    NullableEnumField = SomeEnum.EnumValue3
                },

                #endregion

                #region DecimalField

                new Dummy
                {
                    Id = "170",
                    DecimalField = (decimal) 4.03
                },
                new Dummy
                {
                    Id = "171",
                    DecimalField = (decimal) 6.37
                },

                #endregion

                #region NullableDecimalField

                new Dummy
                {
                    Id = "180",
                    NullableDecimalField = (decimal) 12.09
                },

                #endregion

                #region BooleanField

                new Dummy
                {
                    Id = "190",
                    BooleanField = true
                },
                new Dummy
                {
                    Id = "191",
                    BooleanField = false
                },

                #endregion

                #region NullableBooleanField

                new Dummy
                {
                    Id = "200",
                    NullableBooleanField = false
                },

                #endregion

                #region SByteField

                new Dummy
                {
                    Id = "210",
                    SByteField = 63
                },
                new Dummy
                {
                    Id = "211",
                    SByteField = -89
                },

                #endregion
                
                #region NullableSByteField

                new Dummy
                {
                    Id = "220",
                    NullableSByteField = 91
                },

                #endregion

                #region ByteField

                new Dummy
                {
                    Id = "230",
                    ByteField = 250
                },
                new Dummy
                {
                    Id = "231",
                    ByteField = 130
                },

                #endregion

                #region NullableByteField

                new Dummy
                {
                    Id = "240",
                    NullableByteField = 44
                },

                #endregion

                #region Int16Field

                new Dummy
                {
                    Id = "250",
                    Int16Field = 12345
                },
                new Dummy
                {
                    Id = "251",
                    Int16Field = -23456
                },

                #endregion

                #region NullableInt16Field

                new Dummy
                {
                    Id = "260",
                    NullableInt16Field = 32764
                },

                #endregion

                #region UInt16Field

                new Dummy
                {
                    Id = "270",
                    UInt16Field = 12345
                },
                new Dummy
                {
                    Id = "271",
                    UInt16Field = 45678
                },

                #endregion

                #region NullableUInt16Field

                new Dummy
                {
                    Id = "280",
                    NullableUInt16Field = 65000
                },

                #endregion
                
                #region Int32Field

                new Dummy
                {
                    Id = "290",
                    Int32Field = 100000006
                },
                new Dummy
                {
                    Id = "291",
                    Int32Field = -234567890
                },

                #endregion

                #region NullableInt32Field

                new Dummy
                {
                    Id = "300",
                    NullableInt32Field = 345678901
                },

                #endregion

                #region UInt32Field

                new Dummy
                {
                    Id = "310",
                    UInt32Field = 123456789
                },
                new Dummy
                {
                    Id = "311",
                    UInt32Field = 234567890
                },

                #endregion

                #region NullableUInt32Field

                new Dummy
                {
                    Id = "320",
                    NullableUInt32Field = 345678901
                },

                #endregion
                
                #region Int64Field

                new Dummy
                {
                    Id = "330",
                    Int64Field = 123453489012
                },
                new Dummy
                {
                    Id = "331",
                    Int64Field = -234067890123
                },

                #endregion

                #region NullableInt64Field

                new Dummy
                {
                    Id = "340",
                    NullableInt64Field = 345671901234
                },

                #endregion

                #region UInt64Field

                new Dummy
                {
                    Id = "350",
                    UInt64Field = 123456789012
                },
                new Dummy
                {
                    Id = "351",
                    UInt64Field = 234567890123
                },

                #endregion

                #region NullableUInt64Field

                new Dummy
                {
                    Id = "360",
                    NullableUInt64Field = 345678901234
                },

                #endregion
                
                #region SingleField

                new Dummy
                {
                    Id = "370",
                    SingleField = 21.56901f
                },
                new Dummy
                {
                    Id = "371",
                    SingleField = -34.789f
                },

                #endregion

                #region NullableSingleField

                new Dummy
                {
                    Id = "380",
                    NullableSingleField = 1.3456f
                },

                #endregion
                
                #region DoubleField

                new Dummy
                {
                    Id = "390",
                    DoubleField = 12.3453489012
                },
                new Dummy
                {
                    Id = "391",
                    DoubleField = -2340678.90123
                },

                #endregion

                #region NullableDoubleField

                new Dummy
                {
                    Id = "400",
                    NullableDoubleField = 34567.1901234
                },

                #endregion

                #region Unknown Type
                
                new Dummy
                {
                    Id = "1000",
                    UnknownTypeField = new SomeUnknownType()
                },

                #endregion

                #region ToOneRelatedItem

                new Dummy
                {
                    Id = "1100",
                    ToOneRelatedItem = new RelatedItemWithId
                    {
                        Id = "1101",
                        Name = "Related sample 1"
                    }
                },
                new Dummy
                {
                    Id = "1102",
                    ToOneRelatedItem = new RelatedItemWithId
                    {
                        Id = "1103",
                        Name = "Related sample 2"
                    }
                },

                #endregion
                
                #region ToManyRelatedItems

                new Dummy
                {
                    Id = "1110",
                    ToManyRelatedItems = new List<RelatedItemWithId>
                    {
                        new RelatedItemWithId { Id = "1111", Name = "Related sample 3" },
                        new RelatedItemWithId { Id = "1112", Name = "Related sample 4" }
                    }
                },
                
                new Dummy
                {
                    Id = "1120",
                    ToManyRelatedItems = new List<RelatedItemWithId>
                    {
                        new RelatedItemWithId { Id = "1121", Name = "Related sample 5" },
                        new RelatedItemWithId { Id = "1122", Name = "Related sample 6" }
                    }
                }

                #endregion
            };
            _fixturesQuery = _fixtures.AsQueryable();
        }

        private DefaultFilteringTransformer GetTransformer()
        {
            var pluralizationService = new PluralizationService(new Dictionary<string, string>
            {
                {"Dummy", "Dummies"}
            });
            var modelManager = new ModelManager(pluralizationService);
            modelManager.RegisterResourceType(typeof(Dummy));
            modelManager.RegisterResourceType(typeof(RelatedItemWithId));
            return new DefaultFilteringTransformer(modelManager);
        }

        private Dummy[] GetArray(string uri)
        {
            return Transform(GetTransformer(), _fixturesQuery, uri).ToArray();
        }

        #region String

        [TestMethod]
        public void Filters_by_matching_string_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[stringField]=String value 1");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("100");
        }

        [TestMethod]
        public void Filters_by_missing_string_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[stringField]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 3);
            returnedArray.Any(d => d.Id == "100" || d.Id == "101" || d.Id == "102").Should().BeFalse();
        }

        #endregion

        #region DateTime

        [TestMethod]
        public void Filters_by_matching_datetime_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[dateTimeField]=1930-11-07");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("110");
        }

        [TestMethod]
        public void Filters_by_missing_datetime_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[dateTimeField]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_datetime_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableDateTimeField]=1961-02-18");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("120");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_datetime_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableDateTimeField]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "120").Should().BeFalse();
        }

        #endregion

        #region DateTimeOffset

        [TestMethod]
        public void Filters_by_matching_datetimeoffset_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[dateTimeOffsetField]=1991-01-03");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("130");
        }

        [TestMethod]
        public void Filters_by_missing_datetimeoffset_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[dateTimeOffsetField]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_datetimeoffset_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableDateTimeOffsetField]=2014-05-05");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("140");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_datetimeoffset_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableDateTimeOffsetField]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "140").Should().BeFalse();
        }

        #endregion

        #region Enum

        [TestMethod]
        public void Filters_by_matching_enum_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[enumField]=1");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("150");
        }

        [TestMethod]
        public void Filters_by_missing_enum_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[enumField]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_enum_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableEnumField]=3");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("160");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_enum_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableEnumField]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "160").Should().BeFalse();
        }

        #endregion

        #region Decimal

        [TestMethod]
        public void Filters_by_matching_decimal_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[decimalField]=4.03");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("170");
        }

        [TestMethod]
        public void Filters_by_missing_decimal_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[decimalField]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_decimal_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableDecimalField]=12.09");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("180");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_decimal_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableDecimalField]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "180").Should().BeFalse();
        }

        #endregion

        #region Boolean

        [TestMethod]
        public void Filters_by_matching_boolean_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[booleanField]=true");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("190");
        }

        [TestMethod]
        public void Filters_by_missing_boolean_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[booleanField]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_boolean_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableBooleanField]=false");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("200");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_boolean_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableBooleanField]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "200").Should().BeFalse();
        }

        #endregion

        #region SByte

        [TestMethod]
        public void Filters_by_matching_sbyte_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[sByteField]=63");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("210");
        }

        [TestMethod]
        public void Filters_by_missing_sbyte_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[sByteField]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_sbyte_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableSByteField]=91");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("220");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_sbyte_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableSByteField]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "220").Should().BeFalse();
        }

        #endregion

        #region Byte

        [TestMethod]
        public void Filters_by_matching_byte_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[byteField]=250");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("230");
        }

        [TestMethod]
        public void Filters_by_missing_byte_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[byteField]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_byte_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableByteField]=44");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("240");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_byte_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableByteField]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "240").Should().BeFalse();
        }

        #endregion

        #region Int16

        [TestMethod]
        public void Filters_by_matching_int16_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[int16Field]=12345");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("250");
        }

        [TestMethod]
        public void Filters_by_missing_int16_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[int16Field]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_int16_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableInt16Field]=32764");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("260");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_int16_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableInt16Field]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "260").Should().BeFalse();
        }

        #endregion

        #region UInt16

        [TestMethod]
        public void Filters_by_matching_uint16_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[uInt16Field]=12345");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("270");
        }

        [TestMethod]
        public void Filters_by_missing_uint16_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[uInt16Field]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_uint16_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableUInt16Field]=65000");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("280");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_uint16_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableUInt16Field]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "280").Should().BeFalse();
        }

        #endregion

        #region Int32

        [TestMethod]
        public void Filters_by_matching_int32_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[int32Field]=100000006");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("290");
        }

        [TestMethod]
        public void Filters_by_missing_int32_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[int32Field]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_int32_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableInt32Field]=345678901");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("300");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_int32_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableInt32Field]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "300").Should().BeFalse();
        }

        #endregion

        #region UInt32

        [TestMethod]
        public void Filters_by_matching_uint32_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[uInt32Field]=123456789");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("310");
        }

        [TestMethod]
        public void Filters_by_missing_uint32_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[uInt32Field]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_uint32_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableUInt32Field]=345678901");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("320");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_uint32_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableUInt32Field]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "320").Should().BeFalse();
        }

        #endregion

        #region Int64

        [TestMethod]
        public void Filters_by_matching_int64_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[int64Field]=123453489012");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("330");
        }

        [TestMethod]
        public void Filters_by_missing_int64_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[int64Field]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_int64_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableInt64Field]=345671901234");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("340");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_int64_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableInt64Field]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "340").Should().BeFalse();
        }

        #endregion

        #region UInt64

        [TestMethod]
        public void Filters_by_matching_uint64_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[uInt64Field]=123456789012");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("350");
        }

        [TestMethod]
        public void Filters_by_missing_uint64_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[uInt64Field]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_uint64_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableUInt64Field]=345678901234");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("360");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_uint64_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableUInt64Field]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "360").Should().BeFalse();
        }

        #endregion

        #region Single

        [TestMethod]
        public void Filters_by_matching_single_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[singleField]=21.56901");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("370");
        }

        [TestMethod]
        public void Filters_by_missing_single_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[singleField]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_single_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableSingleField]=1.3456");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("380");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_single_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableSingleField]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "380").Should().BeFalse();
        }

        #endregion

        #region Double

        [TestMethod]
        public void Filters_by_matching_double_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[doubleField]=12.3453489012");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("390");
        }

        [TestMethod]
        public void Filters_by_missing_double_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[doubleField]=");
            returnedArray.Length.Should().Be(0);
        }

        [TestMethod]
        public void Filters_by_matching_nullable_double_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableDoubleField]=34567.1901234");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("400");
        }

        [TestMethod]
        public void Filters_by_missing_nullable_double_property()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[nullableDoubleField]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 1);
            returnedArray.Any(d => d.Id == "400").Should().BeFalse();
        }

        #endregion

        #region Unknown Type

        [TestMethod]
        public void Does_not_filter_unknown_type()
        {
            Action action = () => GetArray("http://api.example.com/dummies?filter[unknownTypeField]=asdfasd");
            action.ShouldThrow<HttpResponseException>().Which.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region To-one relationship

        [TestMethod]
        public void Filters_by_matching_to_one_relationship_id()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[toOneRelatedItem]=1101");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("1100");
        }

        [TestMethod]
        public void Filters_by_missing_to_one_relationship_id()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[toOneRelatedItem]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 2);
            returnedArray.Any(d => d.Id == "1100" || d.Id == "1102").Should().BeFalse();
        }

        #endregion

        #region To-many relationship

        [TestMethod]
        public void Filters_by_matching_id_in_to_many_relationship()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[toManyRelatedItems]=1111");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("1110");
        }

        [TestMethod]
        public void Filters_by_missing_id_in_to_many_relationship()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[toManyRelatedItems]=");
            returnedArray.Length.Should().Be(_fixtures.Count - 2);
            returnedArray.Any(d => d.Id == "1110" || d.Id == "1120").Should().BeFalse();
        }

        #endregion

        #region Multiple filters

        [TestMethod]
        public void Ands_together_filters()
        {
            var returnedArray = GetArray("http://api.example.com/dummies?filter[stringField]=String value 2&filter[enumField]=3");
            returnedArray.Length.Should().Be(1);
            returnedArray[0].Id.Should().Be("102");
        }

        #endregion
    }
}
