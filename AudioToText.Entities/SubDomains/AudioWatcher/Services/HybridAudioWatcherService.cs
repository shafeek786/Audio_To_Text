using AudioToText.Entities.SubDomains.AudioWatcher.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AudioToText.Entities.SubDomains.AudioWatcher.Services;

public class HybridAudioWatcherService:BackgroundService
{
    private readonly IEnumerable<IFileDetectionStrategy> _strategies;
    private readonly ILogger<HybridAudioWatcherService> _logger;

    public HybridAudioWatcherService(
        IEnumerable<IFileDetectionStrategy> strategies,
        ILogger<HybridAudioWatcherService> logger)
    {
        _strategies = strategies;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HybridAudioWatcherService is starting.");
    
        foreach (var strategy in _strategies)
        {
            _logger.LogInformation($"Starting detection strategy: {strategy.GetType().Name}");
        }

        var tasks = _strategies.Select(strategy => strategy.StartDetectionAsync(stoppingToken));
        await Task.WhenAll(tasks);
    }
}