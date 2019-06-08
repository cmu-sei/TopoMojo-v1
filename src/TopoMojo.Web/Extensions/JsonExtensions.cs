// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TopoMojo.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            );
        }

        public static string ToUglyJson(this object obj)
        {
            return JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            );
        }
    }
}
