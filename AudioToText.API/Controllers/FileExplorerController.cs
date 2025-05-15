using AudioToText.Entities.SubDomains.FileExplorer.Interface;
using Microsoft.AspNetCore.Mvc;

namespace AudioToText.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class FileExplorerController: ControllerBase
{
    private readonly IFileExplorerService _fileExplorerService;
    private readonly ICompletedFileExplorerService _completedFileExplorerService;
    private readonly IAudioFileService _audioFileService;
    
    public FileExplorerController(IFileExplorerService fileExplorerService, ICompletedFileExplorerService completedFileExplorerService, IAudioFileService audioFileService)
    {
        _fileExplorerService = fileExplorerService;
        _completedFileExplorerService = completedFileExplorerService;
        _audioFileService = audioFileService;
    }
    
    // [HttpGet("filestructure")]
    // public IActionResult GetFileStructure()
    // {
    //     
    //
    //     var result = _fileExplorerService.GetDirectoryStructure();
    //     return Ok(result);
    // }
    
    [HttpGet("completed-files")]
    public async Task<IActionResult> GetCompletedFiles()
    {
        
        var completedFiles = await _completedFileExplorerService.GetCompletedAudioFilesAsync();
        return Ok(completedFiles); // Return the list as a JSON response
    }
    
    [HttpGet("GetByProcessedGuid/{guid}")]
    public async Task<IActionResult> GetByProcessedGuid(long guid)
    {
        var audioFile = await _audioFileService.GetAudioFileByProcessedGuidAsync(guid);

        if (audioFile == null)
            return NotFound(new { Message = "Audio file not found for the provided GUID." });

        return Ok(audioFile);
    }
    
    [HttpGet("DownloadAudio/{guid}")]
    public async Task<IActionResult> DownloadAudio(long guid)
    {
        var (filePath, fileName) = await _audioFileService.GetAudioFilePathByGuidAsync(guid);

        if (filePath == null || !System.IO.File.Exists(filePath))
        {
            return NotFound(new { Message = "Audio file not found." });
        }

        var contentType = "audio/mpeg"; // Change if using other formats (e.g., "audio/wav")
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(fileStream, contentType, fileName);
    }

}