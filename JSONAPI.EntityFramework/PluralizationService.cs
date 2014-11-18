using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSONAPI.EntityFramework
{
    public class PluralizationService : JSONAPI.Core.IPluralizationService
    {
        private static Lazy<System.Data.Entity.Design.PluralizationServices.PluralizationService> _pls
            = new Lazy<System.Data.Entity.Design.PluralizationServices.PluralizationService>(
                () => System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))
            );
        public string Pluralize(string s)
        {
            return _pls.Value.Pluralize(s);
        }
        public string Singularize(string s)
        {
            return _pls.Value.Singularize(s);
        }
        public bool IsPlural(string s)
        {
            return _pls.Value.IsPlural(s);
        }
        public bool IsSingular(string s)
        {
            return _pls.Value.IsSingular(s);
        }
    }
}
