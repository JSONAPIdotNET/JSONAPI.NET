using System;

namespace JSONAPI.Attributes
{
    /// <summary>
    /// Indicates that the property should be serialized as a complex attribute, instead of a string
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SerializeAsComplexAttribute : Attribute
    {
    }
}
