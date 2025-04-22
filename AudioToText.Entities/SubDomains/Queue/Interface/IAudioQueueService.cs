namespace AudioToText.Entities.SubDomains.Queue.Interface;

public interface IAudioQueueService
{
    void Enqueue(string filePath);
    Task<string> DequeueAsync(CancellationToken cancellationToken);
}