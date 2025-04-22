using AudioToText.Entities.SubDomains.AudioWatcher.Interface;
using AudioToText.Entities.SubDomains.AudioWatcher.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioToText.Entities.SubDomains.AudioWatcher.Services
{
    public class FileSystemWatcherStrategy : IFileDetectionStrategy
    {
        private readonly ILogger<FileSystemWatcherStrategy> _logger;
        private readonly IFileProcessorService _fileProcessorService;
        private readonly AudioWatcherOptions _options;
        private FileSystemWatcher? _watcher;
        private const int MaxAllowedDepth = 1;

        public FileSystemWatcherStrategy(
            ILogger<FileSystemWatcherStrategy> logger,
            IFileProcessorService fileProcessorService,
            IOptions<AudioWatcherOptions> options)
        {
            _logger = logger;
            _fileProcessorService = fileProcessorService;
            _options = options.Value;
            _logger.LogInformation("FileSystemWatcherStrategy created.");
            _logger.LogInformation("Watching path: {Path}", _options.WatchPath);
        }

        public Task StartDetectionAsync(CancellationToken stoppingToken)
        {
            if (!Directory.Exists(_options.WatchPath))
            {
                _logger.LogError("Watch path does not exist: {Path}", _options.WatchPath);
                return Task.CompletedTask;
            }

            _watcher = new FileSystemWatcher(_options.WatchPath, "*.*")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
                IncludeSubdirectories = true
            };

            _watcher.Created += async (s, e) =>
            {
                if (IsInExcludedFolder(e.FullPath))
                {
                    _logger.LogInformation("Skipping file {Path} — in excluded folder.", e.FullPath);
                    return;
                }

                if (!IsWithinAllowedDepth(_options.WatchPath, e.FullPath, MaxAllowedDepth))
                {
                    _logger.LogInformation("File {Path} is too deep in folder structure — skipping.", e.FullPath);
                    return;
                }

                await HandleFileAsync(e.FullPath);
            };

            
            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation("Started FileSystemWatcher on: {Path}", _options.WatchPath);
            return Task.CompletedTask;
        }

        private async Task HandleFileAsync(string filePath)
        {
            const int maxRetries = 10;
            int retries = 0;
            bool fileReady = false;

            // Skip files in 'processed' or 'completed' folders
            if (IsInExcludedFolder(filePath))
            {
                _logger.LogInformation("Skipping file {Path} — already inside a managed folder.", filePath);
                return;
            }

            while (retries < maxRetries && !fileReady)
            {
                try
                {
                    using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        fileReady = true;
                    }
                }
                catch (IOException)
                {
                    retries++;
                    await Task.Delay(1000);
                }
            }

            if (fileReady)
            {
                await _fileProcessorService.ProcessFileAsync(filePath);
            }
            else
            {
                _logger.LogWarning("Failed to process {Path}: File remained locked after retries.", filePath);
            }
        }

        private bool IsInExcludedFolder(string filePath)
        {
            string processedPath = Path.Combine(_options.WatchPath, "processed");
            string completedPath = Path.Combine(_options.WatchPath, "completed");

            return filePath.StartsWith(processedPath, StringComparison.OrdinalIgnoreCase)
                || filePath.StartsWith(completedPath, StringComparison.OrdinalIgnoreCase);
        }
        
        private bool IsWithinAllowedDepth(string rootPath, string filePath, int maxDepth)
        {
            var relativePath = Path.GetRelativePath(rootPath, Path.GetDirectoryName(filePath)!);
            var depth = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Length;
            return depth <= maxDepth;
        }
    }
}
