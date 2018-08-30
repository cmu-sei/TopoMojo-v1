using System;
using System.Linq;

namespace TopoMojo.Core.Models
{
    public class Search
    {
        public string Term { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public string Sort { get; set; }
        public string[] Filters { get; set; } = new string[] {};
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

}
