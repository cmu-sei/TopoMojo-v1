using Newtonsoft.Json;

namespace TopoMojo.Extensions
{
    public static class MappingExtensions
    {
        public static T Map<T>(this object obj)
        {
            return JsonConvert.DeserializeObject<T>(
                JsonConvert.SerializeObject(obj, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore})
            );
        }

    }
}
