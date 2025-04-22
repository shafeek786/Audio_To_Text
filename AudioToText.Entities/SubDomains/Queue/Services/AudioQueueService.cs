using System.Collections.Concurrent;
using AudioToText.Entities.SubDomains.Queue.Interface;
using Microsoft.Extensions.Logging;

namespace AudioToText.Entities.SubDomains.Queue.Services;

public class AudioQueueService: IAudioQueueService
{
    private readonly ConcurrentQueue<string> _audioQueue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly ILogger<AudioQueueService> _logger;
    
    public AudioQueueService(ILogger<AudioQueueService> logger)
    {
        _logger = logger;
    }
    
    public void Enqueue(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Attempted to enqueue non-existent file: {FilePath}", filePath);
            return;
        }

        _audioQueue.Enqueue(filePath);
        _signal.Release();
        _logger.LogInformation("📥 Enqueued file: {FilePath}", filePath);
    }

    public async Task<string?> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);

        if (_audioQueue.TryDequeue(out var filePath))
        {
            return filePath;
        }

        return null;
    }
}