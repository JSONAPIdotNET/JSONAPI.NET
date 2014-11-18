using System;

namespace JSONAPI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class IncludeInPayload : System.Attribute
    {
        internal bool Include;

        public IncludeInPayload(bool include)
        {
            this.Include = include;
        }
    }
}
