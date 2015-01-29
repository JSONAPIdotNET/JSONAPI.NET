using System;

namespace JSONAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SerializeStringAsRawJsonAttribute : Attribute
    {
    }
}
