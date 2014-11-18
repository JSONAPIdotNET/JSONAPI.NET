using System;

namespace JSONAPI.Attributes
{
    public enum SerializeAsOptions { Ids, Link, Embedded }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SerializeAs
        : System.Attribute
    {
        internal SerializeAsOptions How;

        public SerializeAs(SerializeAsOptions how)
        {
            this.How = how;
        }
    }
}
