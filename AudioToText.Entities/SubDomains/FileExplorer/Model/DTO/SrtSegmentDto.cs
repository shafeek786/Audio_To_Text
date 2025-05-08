namespace AudioToText.Entities.SubDomains.FileExplorer.Model.DTO;

public class SrtSegmentDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string TranscriptText { get; set; } = string.Empty;
}