using System;
using JSONAPI.Attributes;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public enum SampleEnum
    {
        Value1 = 1,
        Value2 = 2
    }

    public class Sample
    {
        public string Id { get; set; }
        public Boolean BooleanField { get; set; }
        public Boolean? NullableBooleanField { get; set; }
        public SByte SbyteField { get; set; }
        public SByte? NullableSbyteField { get; set; }
        public Byte ByteField { get; set; }
        public Byte? NullableByteField { get; set; }
        public Int16 Int16Field { get; set; }
        public Int16? NullableInt16Field { get; set; }
        public UInt16 Uint16Field { get; set; }
        public UInt16? NullableUint16Field { get; set; }
        public Int32 Int32Field { get; set; }
        public Int32? NullableInt32Field { get; set; }
        public UInt32 Uint32Field { get; set; }
        public UInt32? NullableUint32Field { get; set; }
        public Int64 Int64Field { get; set; }
        public Int64? NullableInt64Field { get; set; }
        public UInt64 Uint64Field { get; set; }
        public UInt64? NullableUint64Field { get; set; }
        public Double DoubleField { get; set; }
        public Double? NullableDoubleField { get; set; }
        public Single SingleField { get; set; }
        public Single? NullableSingleField { get; set; }
        public Decimal DecimalField { get; set; }
        public Decimal? NullableDecimalField { get; set; }
        public DateTime DateTimeField { get; set; }
        public DateTime? NullableDateTimeField { get; set; }
        public DateTimeOffset DateTimeOffsetField { get; set; }
        public DateTimeOffset? NullableDateTimeOffsetField { get; set; }
        public Guid GuidField { get; set; }
        public Guid? NullableGuidField { get; set; }
        public string StringField { get; set; }
        public SampleEnum EnumField { get; set; }
        public SampleEnum? NullableEnumField { get; set; }

        [SerializeAsComplex]
        public string ComplexAttributeField { get; set; }
    }
}
