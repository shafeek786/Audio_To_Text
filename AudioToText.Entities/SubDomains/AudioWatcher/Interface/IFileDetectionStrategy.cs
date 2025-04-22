namespace AudioToText.Entities.SubDomains.AudioWatcher.Interface;

public interface IFileDetectionStrategy
{
    Task StartDetectionAsync(CancellationToken stoppingToken);
}