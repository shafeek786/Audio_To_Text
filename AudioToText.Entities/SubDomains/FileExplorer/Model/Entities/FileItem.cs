namespace AudioToText.Entities.SubDomains.FileExplorer.Model.Entities;

public class FileItem
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string Type { get; set; }  
    public List<FileItem> Children { get; set; } = new List<FileItem>();
}