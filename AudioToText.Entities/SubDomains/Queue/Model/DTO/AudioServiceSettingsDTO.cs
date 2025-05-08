namespace AudioToText.Entities.SubDomains.Queue.Model.DTO;

public class AudioServiceSettingsDTO
{
    public string UploadUrl { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    
    public string TranscriptionApiUrl { get; set; }
}
