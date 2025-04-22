using AudioToText.Entities.SubDomains.Audio.Modles;

namespace AudioToText.Entities.SubDomains.Audio.Interface;

public interface IAudioRepository
{
    Task<AudioFile> AddAsync(AudioFile audioFile);
    List<AudioFile> GetAll();
}