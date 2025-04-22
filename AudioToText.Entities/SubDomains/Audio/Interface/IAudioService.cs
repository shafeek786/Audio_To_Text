using AudioToText.Entities.SubDomains.Audio.Modles;
using AudioToText.Entities.SubDomains.Audio.Modles.DTO;
using Microsoft.AspNetCore.Http;

namespace AudioToText.Entities.SubDomains.Audio.Interface;

public interface IAudioService
{
    Task<AudioFileDto> ProcessAudioAsync(IFormFile file, string rootPath);
    
    Task<AudioFileDto> ProcessExistingFileAsync(string rootPath);
    
    Task<AudioFileDto> ProcessAudioWithCallbackAsync(IFormFile file, string rootPath, string callbackUrl, string guideId);
    
    Task<Guid> SaveAudioAndGenerateGuideIdAsync(IFormFile file, string rootPath);
    
    Task ProcessAndNotifyAsync(Guid guideId, string filePath, string callbackUrl);
    
    
    List<AudioFileDto> GetAll();
}