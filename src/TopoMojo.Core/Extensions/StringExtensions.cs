using System;
using System.Text.RegularExpressions;

namespace TopoMojo.Core
{
    public static class StringExtensions
    {
        public static bool HasValue(this string s)
        {
            return !String.IsNullOrWhiteSpace(s);
        }

        public static string Before(this string s, char separator)
        {
            int x = s.IndexOf(separator);
            return (x >= 0) ? s.Substring(0, x) : "";
        }

        public static string After(this string s, char separator)
        {
            int x = s.IndexOf(separator);
            return (x >= 0) ? s.Substring(x + 1) : "";
        }

        public static string ToDisplay(this Enum e)
        {
            return e.ToString().Replace("_", " ");
        }

        public static string ExtractUrl(this string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return string.Empty;
            }

            if (Uri.IsWellFormedUriString(inputString, UriKind.Absolute))
            {
                return inputString;
            }

            string pattern = "(href|src)\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";

            Match match = Regex.Match(inputString, pattern,
                                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                                TimeSpan.FromSeconds(1));

            if (match.Success)
            {
                return match.Groups[1].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
