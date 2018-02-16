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