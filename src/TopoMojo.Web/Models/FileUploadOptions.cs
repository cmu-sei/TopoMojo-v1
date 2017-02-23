namespace TopoMojo.Models
{
    public class FileUploadOptions
    {
        public long MaxFileBytes { get; set; }
        public string IsoRoot { get; set; }
        public string TopoRoot { get; set; }
        public string MiscRoot { get; set; }
    }
}