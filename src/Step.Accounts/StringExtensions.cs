using System;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Step.Accounts.Extensions
{
    public static class StringExtensions
    {
        public static string ToHash(this string input)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                return BitConverter.ToString(sha1
                    .ComputeHash(Encoding.UTF8.GetBytes(input.ToLower())))
                    .Replace("-", "")
                    .ToLower();
            }
        }

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

        public static string Extract(this string s, string re)
        {
            Match match = Regex.Match(s, re);
            return match.Groups[match.Groups.Count-1].Value;
        }
    }
}
