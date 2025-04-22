namespace AudioToText.Entities.SubDomains.AudioWatcher.Model;

public class AudioWatcherOptions
{
    public string WatchPath { get; set; } = string.Empty;
    public int PollingIntervalSeconds { get; set; } = 10;
}