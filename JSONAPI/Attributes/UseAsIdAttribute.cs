using System;

namespace JSONAPI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class UseAsIdAttribute : System.Attribute
    {
        public UseAsIdAttribute() { }
    }
}