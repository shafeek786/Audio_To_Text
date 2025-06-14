using AudioToText.Entities.DataBaseContext;
using AudioToText.Entities.SubDomains.Audio.Modles;
using AudioToText.Entities.SubDomains.Callback.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AudioToText.Entities.SubDomains.Callback.Repository
{
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
            _logger.LogInformation($"Callback saved for file repository {payload.Id}");

            
            // Retrieve the audio file based on the provided Guid
            var audioFile = await _dbContext.AudioFiles
                .FirstOrDefaultAsync(x => x.ProcessedFileId == payload.Id);

            if (audioFile == null) return false;

            // Update the audio file with the received transcription and converted date
            audioFile.Transcription = payload.TextResult;
            audioFile.ConvertedAt = payload.ConvertedAt;
            audioFile.Status = "Completed";

            // Parse the SRT text and create SRT segments
            var srtSegments = ParseSrtText(payload.Srt, payload.Id);

            _logger.LogInformation($"Parsed {srtSegments.Count} SRT segments.");

            // Save each segment to the AudioFileSrtSegment table
            await _dbContext.AudioFileSrtSegments.AddRangeAsync(srtSegments);

            // Move the file to the 'completed' directory
            var currentPath = audioFile.AudioFilePath;
            var currentDirectory = Path.GetDirectoryName(currentPath);
            var completedDir = Path.Combine(currentDirectory!, "..", "completed");

            if (!Directory.Exists(completedDir))
                Directory.CreateDirectory(completedDir);

            var completedFilePath = Path.Combine(completedDir, Path.GetFileName(currentPath));

            try
            {
                // If the file exists, move it to the completed directory
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

                // Update the audio file entity and save changes
                _dbContext.AudioFiles.Update(audioFile);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"✅ Audio file updated: {audioFile.FileName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while moving file or updating DB for Guid: {Guid}", payload.Id);
                return false;
            }
        }

       private List<AudioFileSrtSegment> ParseSrtText(string srtText, long processedFileGuid)
{
    var srtSegments = new List<AudioFileSrtSegment>();

    // Split by newline and ensure proper trimming of extra spaces
    var lines = srtText.Split("\n", StringSplitOptions.RemoveEmptyEntries);

    for (int i = 0; i < lines.Length; i++)  // Iterate through each line
    {
        // Check if the line has a valid time frame (start --> end)
        var timeFrame = lines[i].Split("-->");
        if (timeFrame.Length == 2)
        {
            // Remove square brackets and replace comma with period to match TimeSpan format
            var startTimeStr = timeFrame[0].Trim().Replace("[", "").Replace("]", "").Replace(",", ".");
            var endTimeStr = timeFrame[1].Trim().Replace("[", "").Replace("]", "").Replace(",", ".");

            try
            {
                // Parse start and end times using TimeSpan.Parse
                var startTime = TimeSpan.Parse(startTimeStr);
                var endTime = TimeSpan.Parse(endTimeStr);

                // Check if the next line contains transcript text, and handle empty or invalid cases
                var text = i + 1 < lines.Length ? lines[i + 1].Trim() : string.Empty;

                if (!string.IsNullOrEmpty(text))
                {
                    srtSegments.Add(new AudioFileSrtSegment()
                    {
                        ProcessedFileId = processedFileGuid,
                        StartTime = startTime,
                        EndTime = endTime,
                        TranscriptText = text
                    });
                    i++; // Skip the next line since it has already been processed as the transcript text
                }
            }
            catch (FormatException ex)
            {
                // Log and skip any invalid time format
                _logger.LogError(ex, $"Invalid time format in SRT file: {startTimeStr} --> {endTimeStr}");
            }
        }
    }

    return srtSegments;
}




        // Optional method to retrieve saved SRT segments for a specific processed file Guid
        public async Task<List<AudioFileSrtSegment>> GetSrtSegments(long processedFileGuid)
        {
            return await _dbContext.AudioFileSrtSegments
                .Where(x => x.ProcessedFileId == processedFileGuid)
                .ToListAsync();
        }
    }
}
