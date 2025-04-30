namespace AudioToText.Entities.TranscriptionServiceOptions;

public class TranscriptionServiceOptions
{
    public string WebhookUrl { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}
