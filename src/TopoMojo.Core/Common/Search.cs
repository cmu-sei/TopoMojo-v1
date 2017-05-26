using System;
using System.Linq;

namespace TopoMojo.Core
{
    public class Search
    {
        public string Term { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public int Sort { get; set; }
        public SearchFilter[] Filters { get; set; } = new SearchFilter[] {};
    }

    public class SearchResult<T> where T : class
    {
        public Search Search { get; set; }
        public int Total { get; set; }
        public T[] Results { get; set; }
    }

    public class SearchFilter
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public static class SearchExtensions
    {
        public static bool HasFilter(this Search search, string name)
        {
            return search.Filters.Where(f => f.Name.IndexOf(name, StringComparison.CurrentCultureIgnoreCase)>=0).Any();
        }

        public static SearchFilter GetFilter(this Search search, string name)
        {
            return search.Filters.Where(f => f.Name.IndexOf(name, StringComparison.CurrentCultureIgnoreCase)>=0).FirstOrDefault();
        }
    }
}