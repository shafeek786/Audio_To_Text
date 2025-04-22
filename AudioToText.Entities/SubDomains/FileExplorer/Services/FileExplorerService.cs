using AudioToText.Entities.SubDomains.AudioWatcher.Model;
using AudioToText.Entities.SubDomains.FileExplorer.Interface;
using AudioToText.Entities.SubDomains.FileExplorer.Model.Entities;
using Microsoft.Extensions.Options;

namespace AudioToText.Entities.SubDomains.FileExplorer.Services;

public class FileExplorerService : IFileExplorerService
{
    private readonly AudioWatcherOptions _options;
    
    public FileExplorerService(IOptions<AudioWatcherOptions> settings)
    {
        _options = settings.Value;
    }

    public FileItem GetDirectoryStructure()
    {
        return BuildDirectoryStructure(_options.WatchPath);
    }
    
    private FileItem BuildDirectoryStructure(string path)
    {
        var directoryInfo = new DirectoryInfo(path);
        var fileItem = new FileItem
        {
            Name = directoryInfo.Name,
            Path = directoryInfo.FullName,
            Type = "folder"
        };

        try
        {
            // Get directories
            var directories = directoryInfo.GetDirectories()
                .Select(dir => BuildDirectoryStructure(dir.FullName))
                .ToList();

            // Get files
            var files = directoryInfo.GetFiles()
                .Select(file => new FileItem
                {
                    Name = file.Name,
                    Path = file.FullName,
                    Type = "file"
                })
                .ToList();

            fileItem.Children.AddRange(directories);
            fileItem.Children.AddRange(files);
        }
        catch (UnauthorizedAccessException)
        {
            // Log error or skip inaccessible folders
        }

        return fileItem;
    }
}