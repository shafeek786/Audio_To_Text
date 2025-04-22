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
            var directories = Directory.GetDirectories(path);

            foreach (var directory in directories)
            {
                var dirInfo = new DirectoryInfo(directory);
                var completedPath = Path.Combine(directory, "completed");

                var subFolder = new FolderAudioFilesResponse
                {
                    Name = dirInfo.Name,
                    Path = dirInfo.FullName,
                    Type = "folder"
                };

                if (Directory.Exists(completedPath))
                {
                    var completedFiles = Directory.GetFiles(completedPath, "*.mp3");

                    foreach (var filePath in completedFiles)
                    {
                        var fileRecord = await _dbContext.AudioFiles
                            .FirstOrDefaultAsync(f => f.AudioFilePath == filePath && f.Status == "Completed");

                        if (fileRecord != null)
                        {
                            subFolder.
                                Files.Add(new AudioFileMeta
                            {
                                Guid = fileRecord.ProcessedFileGuid,
                                FileName = fileRecord.FileName,
                                FolderPath = fileRecord.AudioFilePath,
                                ReceivedAt = fileRecord.ReceivedAt,
                                ConvertedAt = fileRecord.ConvertedAt,
                                Transcription = fileRecord.Transcription
                            });
                        }
                    }
                }

                await ProcessDirectory(directory, subFolder);

                if (subFolder.Files.Any() || subFolder.SubFolders.Any())
                {
                    parentFolder.SubFolders.Add(subFolder);
                }
            }
        }
    }
}
