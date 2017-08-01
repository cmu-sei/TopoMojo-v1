using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace TopoMojo
{
    public static class JsonAppSettings
    {
        public static void Merge(string path, string sourceFile, string destFile)
        {
            string source = Path.Combine(path, sourceFile);
            string destination = Path.Combine(path, destFile);

            JObject jsrc = JObject.Parse(File.ReadAllText(source));
            string[] canonical = jsrc.Descendants().Where(o=>o.Type == JTokenType.Property)
                .Select(o=>o.Path).ToArray();

            JObject jdst = (File.Exists(destination))
                ? JObject.Parse(File.ReadAllText(destination))
                : new JObject();
            string[] custom = jdst.Descendants().Where(o=>o.Type == JTokenType.Property)
                .Select(o=>o.Path).ToArray();

            string[] newOptions = canonical.Except(custom).ToArray();
            if (newOptions.Length > 0)
            {
                JsonMergeSettings mergeSettings = new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                };

                jdst.Merge(jsrc, mergeSettings);
                File.WriteAllText(destination, jdst.ToString());

                //File.WriteAllLines(Path.Combine(path, "merged-settings.txt"), newOptions);
                Console.WriteLine($"Merged options into {Path.GetFileName(destination)}:");
                foreach (string prop in newOptions)
                    Console.WriteLine(prop);
            }
        }
    }
}
