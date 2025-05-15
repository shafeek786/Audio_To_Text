using AudioToText.Entities.SubDomains.FileExplorer.Model.DTO;

namespace AudioToText.Entities.SubDomains.FileExplorer.Interface;

public interface IAudioFileService
{
    Task<AudioFileMeta?> GetAudioFileByProcessedGuidAsync(long guid);
    Task<(string? FilePath, string? FileName)> GetAudioFilePathByGuidAsync(long guid);
}