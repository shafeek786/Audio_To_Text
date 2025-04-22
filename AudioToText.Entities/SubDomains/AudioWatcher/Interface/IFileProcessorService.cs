namespace AudioToText.Entities.SubDomains.AudioWatcher.Interface;

public interface IFileProcessorService
{
    Task ProcessFileAsync(string filePath);
}