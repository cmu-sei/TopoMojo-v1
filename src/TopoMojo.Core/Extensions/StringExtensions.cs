// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

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

        public static string ExtractBefore(this string s, string target)
        {
            int x = s.IndexOf(target);
            return (x>-1)
                ? s.Substring(0, x)
                : s;
        }
        public static string ExtractAfter(this string s, string target)
        {
            int x = s.IndexOf(target);
            return (x>-1)
                ? s.Substring(x+1)
                : s;
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
