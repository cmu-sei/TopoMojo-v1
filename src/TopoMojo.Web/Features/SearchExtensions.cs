// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;

namespace TopoMojo.Models
{
    public static class SearchExtensions
    {
        public static bool HasFilter(this Search search, string name)
        {
            return search.Filter.Any(f => f == name);
        }

        public static string GetFilter(this Search search, string name)
        {
            return search.Filter.FirstOrDefault(f => f == name);
        }
    }
}
