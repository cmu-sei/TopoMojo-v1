using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TopoMojo.Web
{
    public interface IFileUploadMonitor
    {
        //Task Save(Stream source, Stream dest, long size, string key);
        void Start(string key);
        void Stop(string key);
        void Update(string key, int progress);
        FileProgress Progress(string key);
    }


    public class FileUploadMonitor : IFileUploadMonitor
    {
        public FileUploadMonitor(ILogger<FileUploadMonitor> logger)
        {
            _logger = logger;
            _monitor = new Dictionary<string, FileProgress>();
            CleanupLoop();
        }
        private readonly ILogger<FileUploadMonitor> _logger;
        private Dictionary<string, FileProgress> _monitor;

        // public async Task Save(Stream source, Stream dest, long size, string key)
        // {
        //     if (_Monitor.ContainsKey(key))
        //         throw new Exception("File with this key is already being uploaded.");

        //     _Monitor.Add(key, new FileProgress {
        //         Name = key,
        //         Start = DateTime.UtcNow
        //     });

        //     byte[] buffer = new byte[4096];
        //     int bytes = 0, progress = 0;
        //     long totalBytes = 0, totalBlocks = 0;

        //     do
        //     {
        //         bytes = await source.ReadAsync(buffer, 0, buffer.Length);
        //         await dest.WriteAsync(buffer, 0, bytes);
        //         totalBlocks += 1;
        //         totalBytes += bytes;
        //         if (totalBlocks % 1024 == 0)
        //         {
        //             progress = (int)(((float)totalBytes / (float)size) * 100);
        //             _Monitor[key].Progress = progress;
        //             //Console.WriteLine(progress + "%");
        //         }
        //     } while (bytes > 0);
        //     _Monitor[key].Progress = (int)(((float)totalBytes / (float)size) * 100);
        //     _Monitor[key].Stop = DateTime.UtcNow;
        //     int duration = (int)_Monitor[key].Stop.Subtract(_Monitor[key].Start).TotalSeconds;
        //     _logger.LogInformation($"FileUpload complete for {key} in {duration}s");
        //     ClearKey(key);
        //     //Console.WriteLine(progress + "%");
        // }

        public void Start(string key)
        {
            if (!_monitor.ContainsKey(key))
            {
                _monitor.Add(key, new FileProgress
                {
                    Key = key,
                    Start = DateTime.UtcNow
                });
            }
        }

        public void Stop(string key)
        {
            //await Task.Delay(10000);
            // if (_Monitor.ContainsKey(key))
            // {
            //     _Monitor.Remove(key);
            // }
        }

        public void Update(string key, int progress)
        {
            if (_monitor.ContainsKey(key))
            {
                _monitor[key].Progress = progress;
                _monitor[key].Stop = DateTime.UtcNow;
            }
        }

        public FileProgress Progress(string key)
        {
            if (!_monitor.ContainsKey(key))
                throw new InvalidOperationException();

            return _monitor[key];
        }

        private async Task CleanupLoop()
        {
            while (true)
            {
                DateTime now = DateTime.UtcNow;
                foreach(FileProgress item in _monitor.Values.ToArray())
                {
                    if (now.CompareTo(item.Stop.AddMinutes(2)) > 0)
                    {
                        _logger.LogDebug("removed monitor " + item.Key);
                        _monitor.Remove(item.Key);
                    }
                }
                await Task.Delay(60000);
            }
        }
    }

    public class FileProgress
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public int Progress { get; set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
    }
}