﻿using AudioToText.Entities.DataBaseContext;
using AudioToText.Entities.SubDomains.Audio.Interface;
using AudioToText.Entities.SubDomains.Audio.Modles;
using AudioToText.Entities.SubDomains.Audio.Modles.DTO;

namespace AudioToText.Entities.SubDomains.Audio.Repository;

public class AudioREpository(AudioDbContext context):IAudioRepository
{
    public async Task<AudioFile> AddAsync(AudioFile audioFile)
    {
        context.AudioFiles.Add(audioFile);
        await context.SaveChangesAsync();
        return audioFile;
    }
    
    public List<AudioFile> GetAll()
    {
        return context.AudioFiles.ToList();
    }
    
   
}