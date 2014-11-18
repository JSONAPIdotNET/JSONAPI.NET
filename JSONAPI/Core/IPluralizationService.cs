using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


namespace JSONAPI.Core
{
    /// <summary>
    /// An interface for a pluralization service. It happens to exactly match the methods in 
    /// <see cref="System.Data.EntityDesign.PluralizationServices.PluralizationService"/>.
    /// Go figure.
    /// </summary>
    public interface IPluralizationService
    {
        Boolean IsPlural(string word);
        Boolean IsSingular(string word);
        string Pluralize(string word);
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

        public Boolean IsPlural(string word)
        {
            if (p2s.ContainsKey(word)) return true;
            if (s2p.ContainsKey(word)) return false;
            return word.EndsWith("s");
        }
        public Boolean IsSingular(string word)
        {
            if (s2p.ContainsKey(word)) return true;
            if (p2s.ContainsKey(word)) return false;
            return !word.EndsWith("s");
        }
        public string Pluralize(string word)
        {
            if (s2p.ContainsKey(word)) return s2p[word];
            if (p2s.ContainsKey(word)) return word;
            return word + "s";
        }
        public string Singularize(string word)
        {
            if (p2s.ContainsKey(word)) return p2s[word];
            if (s2p.ContainsKey(word)) return word;
            return word.TrimEnd('s');
        }
    }
}