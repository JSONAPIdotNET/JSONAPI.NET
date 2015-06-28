using System;
using JSONAPI.Core;

namespace JSONAPI.EntityFramework
{
    /// <summary>
    /// Implementation of IPluralizationService that uses EntityFramework's built-in EnglishPluralizationService
    /// </summary>
    public class EntityFrameworkPluralizationService : IPluralizationService
    {
        private static readonly Lazy<System.Data.Entity.Infrastructure.Pluralization.EnglishPluralizationService> _pls
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
