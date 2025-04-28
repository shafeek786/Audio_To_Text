using AudioToText.Entities.SubDomains.Callback.Interface;
using AudioToText.Entities.SubDomains.Callback.Mapper;
using AudioToText.Entities.SubDomains.Callback.Model;
using AudioToText.Entities.SubDomains.Callback.Model.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AudioToText.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CallbackController: ControllerBase
{
    private readonly ICallbackService _callbackService;

    public CallbackController(ICallbackService callbackService)
    {
        _callbackService = callbackService;
    }

    [HttpPost("receive")]
    public async Task<IActionResult> ReceiveCallback([FromBody] TranscriptionResult resultPayload)
    {
        Console.WriteLine(resultPayload);
        Console.WriteLine("******************************************`");
        if (!ModelState.IsValid) return BadRequest($"Invalid data{resultPayload}");
        var payload = resultPayload.ToCallbackPayload();

        var result = await _callbackService.SaveCallbackAsync(payload);
        return result ? Ok("Callback received and stored.") : NotFound("File not found.");
    }
}