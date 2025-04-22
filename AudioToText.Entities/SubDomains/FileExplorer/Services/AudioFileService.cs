using AudioToText.Entities.DataBaseContext;
using AudioToText.Entities.SubDomains.FileExplorer.Interface;
using AudioToText.Entities.SubDomains.FileExplorer.Model.DTO;
using Microsoft.EntityFrameworkCore;


namespace AudioToText.Entities.SubDomains.FileExplorer.Services;

public class AudioFileService: IAudioFileService
{
    private readonly AudioDbContext _context;

    public AudioFileService(AudioDbContext context)
    {
        _context = context;
    }

    public async Task<AudioFileMeta?> GetAudioFileByProcessedGuidAsync(Guid guid)
    {
        return await _context.AudioFiles
            .Where(a => a.ProcessedFileGuid == guid)
            .Select(a => new AudioFileMeta()
            {
                Guid = a.ProcessedFileGuid,      // <-- Add this line
                FileName = a.FileName,
                FolderPath = a.AudioFilePath,
                ReceivedAt = a.ReceivedAt,
                ConvertedAt = a.ConvertedAt,
                Transcription = a.Transcription,
                Status = a.Status
            })
            .FirstOrDefaultAsync();
    }
}