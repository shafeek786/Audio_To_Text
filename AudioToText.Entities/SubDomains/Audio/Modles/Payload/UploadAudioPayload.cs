
using Microsoft.AspNetCore.Http;
namespace AudioToText.Entities.SubDomains.Audio.Modles.Payload;

public class UploadAudioPayload
{
    public IFormFile File { get; set; }
    
    public string CallbackUrl { get; set; }
    
    
}
