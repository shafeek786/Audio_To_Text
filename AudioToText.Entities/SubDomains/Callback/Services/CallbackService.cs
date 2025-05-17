using AudioToText.Entities.SubDomains.Callback.Interface;
using AudioToText.Entities.SubDomains.Callback.Model;
using AudioToText.Entities.SubDomains.Callback.Repository;
using Microsoft.Extensions.Logging;

namespace AudioToText.Entities.SubDomains.Callback.Services;

public class CallbackService:ICallbackService
{
    private readonly CallbackRepository _repository;
    private readonly ILogger<CallbackService> _logger;

    public CallbackService(CallbackRepository repository, ILogger<CallbackService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> SaveCallbackAsync(CallbackPayload payload)
    {
        payload.ConvertedAt = DateTime.UtcNow;
        _logger.LogInformation($"Callback saved for file pauload {payload.Id}");
        try
        {
            var result = await _repository.SaveCallbackAsync(payload);
            if (result)
            {
                _logger.LogInformation($"Callback saved for file {payload} with GUID {payload.Id}");
            }
            else
            {
                _logger.LogWarning($"Audio file {payload.TextResult} not found.");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving callback");
            return false;
        }
    }
}