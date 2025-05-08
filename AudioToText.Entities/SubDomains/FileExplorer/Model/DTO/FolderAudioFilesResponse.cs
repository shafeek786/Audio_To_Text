namespace AudioToText.Entities.SubDomains.FileExplorer.Model.DTO;

public class FolderAudioFilesResponse
{
    public string Name { get; set; } 
    public string Path { get; set; }  
    public string Type { get; set; } = "folder";  
    
    public DateTime ReceiveddAt { get; set; }
    
    public DateTime ConvertedAt { get; set; }
    public List<AudioFileMeta> Children { get; set; } = new();  
    public List<FolderAudioFilesResponse> SubFolders { get; set; } = new List<FolderAudioFilesResponse>();
    public List<AudioFileMeta> Files { get; set; } = new List<AudioFileMeta>();
}

