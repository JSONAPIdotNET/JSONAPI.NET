using System;

namespace JSONAPI.Tests.Models
{
    enum SampleEnum
    {
        Value1 = 1,
        Value2 = 2
    }

    class Sample
    {
        public string Id { get; set; }
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
    }
}
