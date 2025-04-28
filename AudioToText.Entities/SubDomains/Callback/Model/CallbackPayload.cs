namespace AudioToText.Entities.SubDomains.Callback.Model;

public class CallbackPayload
{
    public long Guid { get; set; }
    public string Transcription  { get; set; }
    
    public DateTime ConvertedAt { get; set; }
    
    public string Srt { get; set; }
    
}