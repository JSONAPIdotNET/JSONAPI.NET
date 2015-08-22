using System;
using System.Collections.Generic;


namespace JSONAPI.Core
{
    /// <summary>
    /// A mirror of System.Data.Entity.Infrastructure.Pluralization.IPluralizationService,
    /// but redefined here to allow usage without a dependency on Entity Framework.
    /// </summary>
    public interface IPluralizationService
    {
        /// <summary>
        /// Return the plural form of a word. This function should be idempotent! (Allowing for the caveat that
        /// explicit mappings may be required to make this be true.)
        /// </summary>
        /// <param name="word">The word to pluralize</param>
        /// <returns>The plural form of the word</returns>
        string Pluralize(string word);

        /// <summary>
        /// Return the plural form of a word. This function should be idempotent! (Allowing for the caveat that
        /// explicit mappings may be required to make this be true.)
        /// </summary>
        /// <param name="word">The word to singularize</param>
        /// <returns>The singular form of the word</returns>
        string Singularize(string word);
    }

    /// <summary>
    /// A horribly naive, default implementation of <see cref="IPluralizationService"/>.
    /// If you use this, at least specify mappings extensively!
    /// </summary>
    public class PluralizationService : IPluralizationService
    {
        private Dictionary<string,string> s2p;
        private Dictionary<string,string> p2s;

        public PluralizationService() 
        {
            s2p = new Dictionary<string,string>();
            p2s = new Dictionary<string,string>();
        }

        public PluralizationService(Dictionary<string, string> explicitMappings)
        {
            s2p = new Dictionary<string,string>();
            p2s = new Dictionary<string,string>();
            foreach(KeyValuePair<string,string> pair in explicitMappings)
            {
                s2p.Add(pair.Key, pair.Value);
                p2s.Add(pair.Value, pair.Key);
            }
        }

        public void AddMapping(string singular, string plural)
        {
            if (!s2p.ContainsKey(singular) && !p2s.ContainsKey(plural))
            {
                s2p.Add(singular, plural);
                p2s.Add(plural, singular);
            }
            else
            {
                throw new ArgumentException("At least one side of the mapping already exists.");
            }
        }

        public void RemoveMapping(string singular, string plural)
        {
            if (s2p.ContainsKey(singular) && p2s.ContainsKey(plural) && s2p[singular] == plural && p2s[plural] == singular)
            {
                s2p.Remove(singular);
                p2s.Remove(plural);
            }
            else
            {
                throw new ArgumentException("Specified mapping does not already exist.");
            }
        }

        public string Pluralize(string word)
        {
            if (s2p.ContainsKey(word)) return s2p[word];
            if (p2s.ContainsKey(word)) return word; // idempotence!
            return word + "s";
        }
        public string Singularize(string word)
        {
            if (p2s.ContainsKey(word)) return p2s[word];
            if (s2p.ContainsKey(word)) return word; // idempotentce!
            return word.TrimEnd('s');
        }
    }
}