namespace AudioToText.Entities.SubDomains.FileExplorer.Model.DTO;

public class AudioFileMeta
{
    public long? id { get; set; }
    public string FileName { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public string Transcription { get; set; }
    
    public string FolderPath {get; set;}
    
    public string Type { get; set; }
    
    public string Status { get; set; }
    
    public List<SrtSegmentDto> SrtSegments { get; set; } = new List<SrtSegmentDto>();
}


