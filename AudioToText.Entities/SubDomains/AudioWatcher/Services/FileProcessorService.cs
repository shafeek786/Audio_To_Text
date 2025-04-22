using AudioToText.Entities.SubDomains.AudioWatcher.Interface;
using AudioToText.Entities.SubDomains.Queue.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AudioToText.Entities.SubDomains.AudioWatcher.Services;

public class FileProcessorService: IFileProcessorService
{
    private readonly ILogger<FileProcessorService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public FileProcessorService(
        ILogger<FileProcessorService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task ProcessFileAsync(string filePath)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var queueService = scope.ServiceProvider.GetRequiredService<IAudioQueueService>();
            queueService.Enqueue(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {File}", filePath);
        }
    }
}