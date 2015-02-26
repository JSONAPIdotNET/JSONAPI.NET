using System;
using JSONAPI.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.Extensions
{
    [TestClass]
    public class TypeExtensionsTests
    {
        private enum TestEnum
        {

        }

        [TestMethod]
        public void CanWriteAsJsonApiAttributeTest()
        {
            Assert.IsTrue(typeof(Byte).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for Byte!");
            Assert.IsTrue(typeof(Byte?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable Byte!");
            Assert.IsTrue(typeof(SByte).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for SByte!");
            Assert.IsTrue(typeof(SByte?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable SByte!");
            Assert.IsTrue(typeof(UInt16).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for UInt16!");
            Assert.IsTrue(typeof(UInt16?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable UInt16!");
            Assert.IsTrue(typeof(Int16).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for Int16!");
            Assert.IsTrue(typeof(Int16?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable Int16!");
            Assert.IsTrue(typeof(UInt32).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for UInt32!");
            Assert.IsTrue(typeof(UInt32?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable UInt32!");
            Assert.IsTrue(typeof(Int32).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for Int32!");
            Assert.IsTrue(typeof(Int32?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable Int32!");
            Assert.IsTrue(typeof(UInt64).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for UInt64!");
            Assert.IsTrue(typeof(UInt64?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable UInt64!");
            Assert.IsTrue(typeof(Int64).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for Int64!");
            Assert.IsTrue(typeof(Int64?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable Int64!");
            Assert.IsTrue(typeof(Double).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for Double!");
            Assert.IsTrue(typeof(Double?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable Double!");
            Assert.IsTrue(typeof(Single).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for Single!");
            Assert.IsTrue(typeof(Single?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable Single!");
            Assert.IsTrue(typeof(Decimal).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for Decimal!");
            Assert.IsTrue(typeof(Decimal?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable Decimal!");
            Assert.IsTrue(typeof(DateTime).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for DateTime!");
            Assert.IsTrue(typeof(DateTime?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable DateTime!");
            Assert.IsTrue(typeof(DateTimeOffset).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for DateTimeOffset!");
            Assert.IsTrue(typeof(DateTimeOffset?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable DateTimeOffset!");
            Assert.IsTrue(typeof(Guid).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for Guid!");
            Assert.IsTrue(typeof(Guid?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable Guid!");
            Assert.IsTrue(typeof(String).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for String!");
            Assert.IsTrue(typeof(TestEnum).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for enum!");
            Assert.IsTrue(typeof(TestEnum?).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for nullable enum!");
            Assert.IsFalse(typeof(Object).CanWriteAsJsonApiAttribute(), "CanWriteTypeAsAttribute returned wrong answer for Object!");
        }

    }
}
