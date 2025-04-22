using AudioToText.Entities.SubDomains.FileExplorer.Model.Entities;

namespace AudioToText.Entities.SubDomains.FileExplorer.Interface;

public interface IFileExplorerService
{
    FileItem GetDirectoryStructure();
}