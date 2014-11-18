using System;

namespace JSONAPI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class LinkTemplate : System.Attribute
    {
        internal string TemplateString;

        public LinkTemplate(string linkTemplate)
        {
            this.TemplateString = linkTemplate;
        }
    }
}
