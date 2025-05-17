using AudioToText.Entities.SubDomains.Callback.Interface;
using AudioToText.Entities.SubDomains.Callback.Model;
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
    public async Task<IActionResult> ReceiveCallback([FromBody] CallbackPayload payload)
    {
        //if (!ModelState.IsValid) return BadRequest("Invalid data");

        var result = await _callbackService.SaveCallbackAsync(payload);
        return result ? Ok("Callback received and stored.") : NotFound("File not found.");
    }
}