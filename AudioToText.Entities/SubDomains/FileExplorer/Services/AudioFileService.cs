using AudioToText.Entities.DataBaseContext;
using AudioToText.Entities.SubDomains.FileExplorer.Interface;
using AudioToText.Entities.SubDomains.FileExplorer.Model.DTO;
using AudioToText.Entities.SubDomains.Audio.Modles;
using Microsoft.EntityFrameworkCore;

namespace AudioToText.Entities.SubDomains.FileExplorer.Services
{
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
                .Include(a => a.AudioFileSrtSegments)  // Include the related SRT segments
                .Select(a => new AudioFileMeta()
                {
                    Guid = a.ProcessedFileGuid,
                    FileName = a.FileName,
                    FolderPath = a.AudioFilePath,
                    ReceivedAt = a.ReceivedAt,
                    ConvertedAt = a.ConvertedAt,
                    Transcription = a.Transcription,
                    Status = a.Status,
                    SrtSegments = a.AudioFileSrtSegments.Select(srt => new SrtSegmentDto
                    {
                        StartTime = srt.StartTime,
                        EndTime = srt.EndTime,
                        TranscriptText = srt.TranscriptText
                    }).ToList()  // Map the SRT segments to DTO
                })
                .FirstOrDefaultAsync();
        }
        
        public async Task<(string? FilePath, string? FileName)> GetAudioFilePathByGuidAsync(Guid guid)
        {
            var audioFile = await _context.AudioFiles
                .Where(a => a.ProcessedFileGuid == guid)
                .Select(a => new { a.AudioFilePath, a.FileName })
                .FirstOrDefaultAsync();

            if (audioFile == null)
                return (null, null);

            
            Console.WriteLine("Resolved Path: " + audioFile.AudioFilePath); 
            return (audioFile.AudioFilePath, audioFile.FileName);
        }
        

    }
}