// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

namespace TopoMojo.Web
{
    public class CacheOptions
    {
        public string Key { get; set; }
        public string RedisUrl { get; set; }
        public string SharedFolder { get; set; }
        public string DataProtectionFolder { get; set; } = ".dpk";
        public int CacheExpirationSeconds { get; set; } = 300;
    }
}
