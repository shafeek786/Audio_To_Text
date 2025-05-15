using AudioToText.Entities.DataBaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AudioToText.Entities.SubDomains.CallbackFailure.Services;

public class CallbackFailureMonitorService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CallbackFailureMonitorService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public CallbackFailureMonitorService(
        IServiceProvider services,
        ILogger<CallbackFailureMonitorService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _services = services;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AudioDbContext>();
            var httpClient = _httpClientFactory.CreateClient();

            var stuckFiles = await db.AudioFiles
                .Where(a => a.Status == "Processed" 
                            && a.Transcription == null 
                            && a.ConvertedAt <= DateTime.UtcNow.AddHours(-2))
                .ToListAsync(stoppingToken);

            foreach (var audio in stuckFiles)
            {
                try
                {
                    _logger.LogWarning("🚨 File {FileName} not transcribed after 2 hours. Checking status...", audio.FileName);

                    var statusCheckResponse = await httpClient.GetAsync($"https://your-api-server/api/Audio/status/{audio.ProcessedFileId}", stoppingToken);

                    if (statusCheckResponse.IsSuccessStatusCode)
                    {
                        var status = await statusCheckResponse.Content.ReadAsStringAsync();

                        if (status.Contains("Failed", StringComparison.OrdinalIgnoreCase) ||
                            status.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("❌ File {FileName} status: {Status}. Re-sending for conversion.", audio.FileName, status);

                            var fileBytes = await File.ReadAllBytesAsync(audio.AudioFilePath, stoppingToken);
                            using var memoryStream = new MemoryStream(fileBytes);
                            using var formContent = new MultipartFormDataContent();
                            var fileContent = new StreamContent(memoryStream);
                            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(GetContentType(Path.GetExtension(audio.FileName)));

                            formContent.Add(fileContent, "File", audio.FileName);
                            formContent.Add(new StringContent("https://localhost:44365/api/Callback/receive"), "CallbackUrl");

                            var resendResponse = await httpClient.PostAsync("https://localhost:44365/api/Audio/upload", formContent, stoppingToken);

                            if (resendResponse.IsSuccessStatusCode)
                            {
                                audio.ConvertedAt = DateTime.UtcNow;
                                audio.Status = "Reprocessing";
                                audio.RetryCount += 1;

                                _logger.LogInformation("🔁 File {FileName} re-sent for conversion successfully.", audio.FileName);
                            }
                            else
                            {
                                _logger.LogError("⚠️ Failed to re-send {FileName}. Status: {StatusCode}", audio.FileName, resendResponse.StatusCode);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("✅ Server reports: {FileName} is still processing. No action taken.", audio.FileName);
                        }
                    }
                    else
                    {
                        _logger.LogError("🛑 Failed to check status for {FileName}. Status code: {StatusCode}", audio.FileName, statusCheckResponse.StatusCode);
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while handling callback check for {FileName}", audio.FileName);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    private string GetContentType(string extension)
    {
        return extension.ToLower() switch
        {
            ".wav" => "audio/wav",
            ".mp3" => "audio/mpeg",
            ".m4a" => "audio/mp4",
            _ => "application/octet-stream"
        };
    }
}
