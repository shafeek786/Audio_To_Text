using AudioToText.Entities.SubDomains.AudioWatcher.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioToText.Entities.SubDomains.AudioWatcher.Interface;

public class PollingDetectionStrategy: IFileDetectionStrategy
{
    private readonly ILogger<PollingDetectionStrategy> _logger;
    private readonly IFileProcessorService _fileProcessorService;
    private readonly AudioWatcherOptions _options;
    private const int MaxAllowedDepth = 1;

    public PollingDetectionStrategy(
        ILogger<PollingDetectionStrategy> logger,
        IFileProcessorService fileProcessorService,
        IOptions<AudioWatcherOptions> options)
    {
        _logger = logger;
        _fileProcessorService = fileProcessorService;
        _options = options.Value;
        _logger.LogInformation("PollingDetectionStrategy initialized. Watching path: {Path}", _options.WatchPath);
     
    }

    public async Task StartDetectionAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (Directory.Exists(_options.WatchPath))
                {
                    var files = Directory.GetFiles(_options.WatchPath, "*.*", SearchOption.AllDirectories);
                    foreach (var filePath in files)
                    {
                        if (IsInExcludedFolder(filePath) || !IsWithinAllowedDepth(_options.WatchPath, filePath, MaxAllowedDepth))
                        {
                            _logger.LogInformation("Skipping file {Path} — located in a managed folder.", filePath);
                            continue;
                        }
                       
                            await _fileProcessorService.ProcessFileAsync(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Polling error occurred.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
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