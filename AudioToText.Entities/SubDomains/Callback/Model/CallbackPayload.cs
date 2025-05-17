namespace AudioToText.Entities.SubDomains.Callback.Model;

public class CallbackPayload
{
    public long Id { get; set; }
    public string TextResult  { get; set; }
    
    public DateTime? ConvertedAt { get; set; } = DateTime.UtcNow;
    
    public string srt { get; set; }
    
}