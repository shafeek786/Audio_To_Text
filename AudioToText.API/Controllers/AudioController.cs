using AudioToText.Entities.SubDomains.Audio.Interface;
using AudioToText.Entities.SubDomains.Audio.Modles.Payload;
using Microsoft.AspNetCore.Mvc;

namespace AudioToText.API.Controllers;

[ApiController]
[Route("/api/[controller]")]

public class AudioController: ControllerBase
{
    private readonly ILogger<AudioController> _logger;
    private readonly IAudioService _audioService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    

    public AudioController(IAudioService audioService, ILogger<AudioController> logger, IWebHostEnvironment webHostEnvironment)
    {
        _audioService = audioService;
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        return Ok(new { Message = "Test endpoint reached successfully." });
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] UploadAudioPayload payload)
    // public async Task<IActionResult> UploadWithCallback([FromForm] IFormFile file, [FromForm] string callbackUrl)
    
    {
        if (payload.File == null || string.IsNullOrWhiteSpace(payload.CallbackUrl))
        {
            return BadRequest("File and Callback URL are required.");
        }

        string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        var guideId = await _audioService.SaveAudioAndGenerateGuideIdAsync(payload.File, rootPath);

        // Start processing asynchronously
        _ = _audioService.ProcessAndNotifyAsync(guideId, Path.Combine(rootPath, payload.File.FileName), payload.CallbackUrl);

        return Ok(new { GuideId = guideId, Message = "File received. Processing started." });
    }


    
    [HttpGet("all")]
    public IActionResult All()
    {
        return Ok(_audioService.GetAll());
    }
    
}