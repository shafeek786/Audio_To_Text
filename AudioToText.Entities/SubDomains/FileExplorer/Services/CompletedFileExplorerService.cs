using AudioToText.Entities.DataBaseContext;
using AudioToText.Entities.SubDomains.FileExplorer.Model.DTO;
using Microsoft.EntityFrameworkCore;
using AudioToText.Entities.SubDomains.FileExplorer.Interface;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AudioToText.Entities.SubDomains.FileExplorer.Services
{
    public class CompletedFileExplorerService : ICompletedFileExplorerService
    {
        private readonly AudioDbContext _dbContext;
        private readonly string _rootPath;

        public CompletedFileExplorerService(AudioDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _rootPath = configuration["AudioWatcher:WatchPath"];
        }

        public async Task<List<FolderAudioFilesResponse>> GetCompletedAudioFilesAsync()
        {
            var rootFolderResponse = new FolderAudioFilesResponse
            {
                Name = new DirectoryInfo(_rootPath).Name,
                Path = _rootPath,
                Type = "folder"
            };

            await ProcessDirectory(_rootPath, rootFolderResponse);
            return new List<FolderAudioFilesResponse> { rootFolderResponse };
        }

        private async Task ProcessDirectory(string path, FolderAudioFilesResponse parentFolder)
        {
            // ✅ Check for "completed" folder in current directory
            var completedPath = Path.Combine(path, "completed");

            if (Directory.Exists(completedPath))
            {
                var completedFiles = Directory.GetFiles(completedPath)
                    .Where(f => IsSupportedAudioFile(f))
                    .ToArray();


                foreach (var filePath in completedFiles)
                {
                    var fileRecord = await _dbContext.AudioFiles
                        .FirstOrDefaultAsync(f => f.AudioFilePath == filePath && f.Status == "Completed");

                    if (fileRecord != null)
                    {
                        parentFolder.Files.Add(new AudioFileMeta
                        {
                            id = fileRecord.ProcessedFileId,
                            FileName = fileRecord.FileName,
                            FolderPath = fileRecord.AudioFilePath,
                            ReceivedAt = fileRecord.ReceivedAt,
                            ConvertedAt = fileRecord.ConvertedAt,
                            Transcription = fileRecord.Transcription
                        });
                    }
                }
            }

            // 🔁 Recurse into subdirectories
            var directories = Directory.GetDirectories(path);

            foreach (var directory in directories)
            {
                var dirInfo = new DirectoryInfo(directory);
                var subFolder = new FolderAudioFilesResponse
                {
                    Name = dirInfo.Name,
                    Path = dirInfo.FullName,
                    Type = "folder"
                };

                await ProcessDirectory(directory, subFolder);

                if (subFolder.Files.Any() || subFolder.SubFolders.Any())
                {
                    parentFolder.SubFolders.Add(subFolder);
                }
            }
        }
        private static readonly HashSet<string> SupportedAudioExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a", ".wma", ".aiff", ".opus"
        };

        private bool IsSupportedAudioFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return SupportedAudioExtensions.Contains(extension);
        }

    }
}
