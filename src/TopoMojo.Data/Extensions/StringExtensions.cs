// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Data
{
    public static class StringExtensions
    {
        public static bool HasValue(this string s)
        {
            return !System.String.IsNullOrWhiteSpace(s);
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
    }
}
