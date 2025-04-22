using AudioToText.Entities.SubDomains.Callback.Model;

namespace AudioToText.Entities.SubDomains.Callback.Interface;

public interface ICallbackService
{
    Task<bool> SaveCallbackAsync(CallbackPayload payload);
}