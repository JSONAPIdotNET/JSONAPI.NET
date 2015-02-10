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
    }
}
