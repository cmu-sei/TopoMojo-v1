using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;

namespace TopoMojo
{
    // public interface IFileUploadManager
    // {
    //     Task Save(Stream source, Stream dest, long size, string key);
    //     int CheckProgress(string key);
    // }

    public class FileUploadManager : IFileUploadManager
    {
        public FileUploadManager(ILogger<FileUploadManager> logger)
        {
            _logger = logger;
            _tracker = new Dictionary<string, FileProgress>();
        }
        private readonly ILogger<FileUploadManager> _logger;
        private Dictionary<string, FileProgress> _tracker;

        public async Task Save(Stream source, Stream dest, long size, string key)
        {
            if (_tracker.ContainsKey(key))
                throw new Exception("File with this key is already being uploaded.");

            _tracker.Add(key, new FileProgress {
                Name = key,
                Start = DateTime.UtcNow
            });

            byte[] buffer = new byte[4096];
            int bytes = 0, progress = 0;
            long totalBytes = 0, totalBlocks = 0;

            do
            {
                bytes = await source.ReadAsync(buffer, 0, buffer.Length);
                await dest.WriteAsync(buffer, 0, bytes);
                totalBlocks += 1;
                totalBytes += bytes;
                if (totalBlocks % 1024 == 0)
                {
                    progress = (int)(((float)totalBytes / (float)size) * 100);
                    _tracker[key].Progress = progress;
                    //Console.WriteLine(progress + "%");
                }
            } while (bytes > 0);
            _tracker[key].Progress = (int)(((float)totalBytes / (float)size) * 100);
            _tracker[key].Stop = DateTime.UtcNow;
            int duration = (int)_tracker[key].Stop.Subtract(_tracker[key].Start).TotalSeconds;
            _logger.LogInformation($"FileUpload complete for {key} in {duration}s");
            ClearKey(key);
            //Console.WriteLine(progress + "%");
        }

        private async Task ClearKey(string key)
        {
            await Task.Delay(10000);
            if (_tracker.ContainsKey(key))
            {
                _tracker.Remove(key);
            }
        }

        public int CheckProgress(string key)
        {
            if (_tracker.ContainsKey(key))
            {
                return _tracker[key].Progress;
            }
            return -1;
        }

        internal class FileProgress
        {
            public string Name { get; set; }
            public int Progress { get; set; }
            public DateTime Start { get; set; }
            public DateTime Stop { get; set; }
        }
    }
}