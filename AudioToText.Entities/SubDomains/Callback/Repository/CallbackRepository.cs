using AudioToText.Entities.DataBaseContext;
using AudioToText.Entities.SubDomains.Callback.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AudioToText.Entities.SubDomains.Callback.Repository;

public class CallbackRepository
{
    private readonly AudioDbContext _dbContext;
    private readonly ILogger<CallbackRepository> _logger;

    public CallbackRepository(AudioDbContext dbContext, ILogger<CallbackRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> SaveCallbackAsync(CallbackPayload payload)
    {
        _logger.LogInformation($"Callback saved for file repository {payload.Guid} {payload.Transcription} ");
        var audioFile = await _dbContext.AudioFiles
            .FirstOrDefaultAsync(x => x.ProcessedFileGuid == payload.Guid);

        if (audioFile == null) return false;

        audioFile.Transcription = payload.Transcription;
        audioFile.ConvertedAt = payload.ConvertedAt;  
        audioFile.SrtText = payload.Srt; 
        audioFile.Status = "Completed";

        var currentPath = audioFile.AudioFilePath;
        var currentDirectory = Path.GetDirectoryName(currentPath);
        var completedDir = Path.Combine(currentDirectory!, "..", "completed");

        if (!Directory.Exists(completedDir))
            Directory.CreateDirectory(completedDir);

        var completedFilePath = Path.Combine(completedDir, Path.GetFileName(currentPath));

        try
        {
            if (File.Exists(currentPath))
            {
                File.Move(currentPath, completedFilePath, overwrite: true);
                _logger.LogInformation($"📂 File moved to completed folder: {completedFilePath}");

                // Update the file path in the database
                audioFile.AudioFilePath = Path.GetFullPath(completedFilePath);
            }
            else
            {
                _logger.LogWarning($"⚠️ File not found at path: {currentPath}");
            }

            _dbContext.AudioFiles.Update(audioFile);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"✅ Audio file updated: {audioFile.FileName}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error while moving file or updating DB for Guid: {Guid}", payload.Guid);
            return false;
        }
    }

}