namespace AudioToText.Entities.SubDomains.Audio.Modles.DTO;

public class TranscriptionResponse
{
    public long id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string WebhookUrl { get; set; } = string.Empty;
    public long TenantId { get; set; }
    public string TranscriptionType { get; set; } = string.Empty;

}