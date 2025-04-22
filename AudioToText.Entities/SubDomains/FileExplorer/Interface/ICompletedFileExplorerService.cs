using AudioToText.Entities.SubDomains.FileExplorer.Model.DTO;

namespace AudioToText.Entities.SubDomains.FileExplorer.Interface;

public interface ICompletedFileExplorerService
{
    Task<List<FolderAudioFilesResponse>> GetCompletedAudioFilesAsync();
}