using System;
using System.IO;
using System.Threading.Tasks;

namespace TopoMojo.Abstractions
{
    public interface IFileUploadManager
    {
        Task Save(Stream source, Stream dest, long size, string key);
        int CheckProgress(string key);
    }

    public class FileUploadConfiguration
    {
        public long MaxFileBytes { get; set; }
        public string IsoRoot { get; set; }
        public string TopoRoot { get; set; }
        public string MiscRoot { get; set; }
    }
}