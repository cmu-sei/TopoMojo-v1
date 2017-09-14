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