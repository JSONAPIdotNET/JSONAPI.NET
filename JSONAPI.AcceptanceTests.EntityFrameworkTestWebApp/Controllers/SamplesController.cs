using System;
using System.Web.Http;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Controllers
{
    public class SamplesController : ApiController
    {
        public IHttpActionResult GetSamples()
        {
            var s1 = new Sample
            {
                Id = "1",
                BooleanField = false,
                NullableBooleanField = false,
                SbyteField = default(SByte),
                NullableSbyteField = null,
                ByteField = default(Byte),
                NullableByteField = null,
                Int16Field = default(Int16),
                NullableInt16Field = null,
                Uint16Field = default(UInt16),
                NullableUint16Field = null,
                Int32Field = default(Int32),
                NullableInt32Field = null,
                Uint32Field = default(Int32),
                NullableUint32Field = null,
                Int64Field = default(Int64),
                NullableInt64Field = null,
                Uint64Field = default(UInt64),
                NullableUint64Field = null,
                DoubleField = default(Double),
                NullableDoubleField = null,
                SingleField = default(Single),
                NullableSingleField = null,
                DecimalField = default(Decimal),
                NullableDecimalField = null,
                DateTimeField = default(DateTime),
                NullableDateTimeField = null,
                DateTimeOffsetField = default(DateTimeOffset),
                NullableDateTimeOffsetField = null,
                GuidField = default(Guid),
                NullableGuidField = null,
                StringField = null,
                EnumField = default(SampleEnum),
                NullableEnumField = null,
                ComplexAttributeField = null
            };
            var s2 = new Sample
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

            return Ok(new[] { s1, s2 });
        }
    }
}