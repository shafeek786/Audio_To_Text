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
    
    [HttpGet("filestructure")]
    public IActionResult GetFileStructure()
    {
        

        var result = _fileExplorerService.GetDirectoryStructure();
        return Ok(result);
    }
    
    [HttpGet("completed-files")]
    public async Task<IActionResult> GetCompletedFiles()
    {
        
        var completedFiles = await _completedFileExplorerService.GetCompletedAudioFilesAsync();
        return Ok(completedFiles); // Return the list as a JSON response
    }
    
    [HttpGet("GetByProcessedGuid/{guid}")]
    public async Task<IActionResult> GetByProcessedGuid(Guid guid)
    {
        var audioFile = await _audioFileService.GetAudioFileByProcessedGuidAsync(guid);

        if (audioFile == null)
            return NotFound(new { Message = "Audio file not found for the provided GUID." });

        return Ok(audioFile);
    }
    
}