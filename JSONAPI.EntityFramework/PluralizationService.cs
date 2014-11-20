using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSONAPI.EntityFramework
{
    public class PluralizationService : JSONAPI.Core.IPluralizationService
    {
        private static Lazy<System.Data.Entity.Infrastructure.Pluralization.EnglishPluralizationService> _pls
            = new Lazy<System.Data.Entity.Infrastructure.Pluralization.EnglishPluralizationService>(
                () => new System.Data.Entity.Infrastructure.Pluralization.EnglishPluralizationService()
            );
        public string Pluralize(string s)
        {
            return _pls.Value.Pluralize(s);
        }
        public string Singularize(string s)
        {
            return _pls.Value.Singularize(s);
        }
    }
}
