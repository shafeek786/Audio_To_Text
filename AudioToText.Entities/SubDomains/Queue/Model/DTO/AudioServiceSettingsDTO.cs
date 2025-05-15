namespace AudioToText.Entities.SubDomains.Queue.Model.DTO;

public class AudioServiceSettingsDTO
{
    public string UploadUrl { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    
    public string TenantId { get; set; } = string.Empty;
    
    
}
