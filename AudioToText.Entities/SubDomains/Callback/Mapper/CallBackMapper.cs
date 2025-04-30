using AudioToText.Entities.SubDomains.Callback.Model.DTO;
using AudioToText.Entities.SubDomains.Callback.Model;

namespace AudioToText.Entities.SubDomains.Callback.Mapper;

public static class CallBackMapper
{
    public static CallbackPayload ToCallbackPayload(this TranscriptionResult result)
    {
        return new CallbackPayload
        {
            Guid = result.Id,
            Transcription = result.TextResult,
            ConvertedAt = result.ProcessedAt,
            Srt = string.Empty
        };
    }
}