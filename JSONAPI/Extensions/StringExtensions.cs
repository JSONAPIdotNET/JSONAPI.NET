using System.Text.RegularExpressions;

namespace JSONAPI.Extensions
{
    internal static class StringExtensions
    {
        private static readonly Regex PascalizeRegex = new Regex(@"(?:^|_|\-|\.)(.)");

        public static string Pascalize(this string word)
        {
            return PascalizeRegex.Replace(
                word,
                match => match.Groups[1].Value.ToUpper());
        }

        public static string Depascalize(this string word)
        {
            return Regex.Replace(
                Regex.Replace(
                    Regex.Replace(word, @"([A-Z]+)([A-Z][a-z])", "$1_$2"), @"([a-z\d])([A-Z])",
                    "$1_$2"), @"[-\s]", "_").ToLower();
        }

        public static string Dasherize(this string word)
        {
            return Depascalize(word).Replace('_', '-');
        }
    }
}
