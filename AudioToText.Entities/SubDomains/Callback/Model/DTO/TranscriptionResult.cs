namespace AudioToText.Entities.SubDomains.Callback.Model.DTO;

public class TranscriptionResult
{
    public string OriginalFileName { get; set; }
    public string? TextResult { get; set; }
    public double? AudioLength { get; set; } 
    public string Status { get; set; }
    public string? TranscriptionType { get; set; }
    public DateTime ProcessedAt { get; set; }
    
    public long Id { get; set; }
}