// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;

namespace TopoMojo.Core.Models.Extensions
{
    public static class SearchExtensions
    {
        public static bool HasFilter(this Search search, string name)
        {
            return search.Filters.Where(f => f.IndexOf(name, StringComparison.CurrentCultureIgnoreCase)>=0).Any();
        }

        public static string GetFilter(this Search search, string name)
        {
            return search.Filters.Where(f => f.IndexOf(name, StringComparison.CurrentCultureIgnoreCase)>=0).FirstOrDefault();
        }
    }
}
