using System.Text.RegularExpressions;

namespace JSONAPI.Tests.Json
{
    static class JsonHelpers
    {
        // http://stackoverflow.com/questions/8913138/minify-indented-json-string-in-net
        public static string MinifyJson(string input)
        {
            return Regex.Replace(input, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
        }
    }
}
