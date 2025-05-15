using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AudioToText.Entities.SubDomains.Audio.Interface;
using AudioToText.Entities.SubDomains.Queue.Interface;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using AudioToText.Entities.SubDomains.Audio.Modles;
using AudioToText.Entities.SubDomains.Callback.Model.DTO;
using AudioToText.Entities.SubDomains.Queue.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace AudioToText.Entities.SubDomains.Audio.Services
{
    public class AudioQueueProcessorService : BackgroundService
    {
        private readonly ILogger<AudioQueueProcessorService> _logger;
        private readonly IServiceProvider _services;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AudioServiceSettingsDTO _settings;

        public AudioQueueProcessorService(ILogger<AudioQueueProcessorService> logger, 
            IServiceProvider services, 
            IHttpClientFactory httpClientFactory,
            IOptions<AudioServiceSettingsDTO> settings)
        {
            _logger = logger;
            _services = services;
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🎯 AudioQueueProcessorService started.");

            using var scope = _services.CreateScope();
            var queue = scope.ServiceProvider.GetRequiredService<IAudioQueueService>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var filePath = await queue.DequeueAsync(stoppingToken);
                if (filePath != null)
                {
                    try
                    {
                        _logger.LogInformation($"file path {filePath}");
                        var fileBytes = await File.ReadAllBytesAsync(filePath);
                        using var memoryStream = new MemoryStream(fileBytes);
                        var fileName = Path.GetFileName(filePath);
                        _logger.LogInformation($"file path {fileName}");
                        using var formContent = new MultipartFormDataContent();
                        var fileContent = new StreamContent(memoryStream);
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(GetContentType(Path.GetExtension(filePath)));

                        formContent.Add(fileContent, "AudioFile", fileName);
                        var client = _httpClientFactory.CreateClient();
                        client.DefaultRequestHeaders.Add("TenantId", _settings.TenantId);
                        var response = await client.PostAsync(_settings.UploadUrl, formContent, stoppingToken);
                        if (response.IsSuccessStatusCode)
                        {
                            var resultJson = await response.Content.ReadAsStringAsync();
                            var result = JsonSerializer.Deserialize<UploadResponse>(resultJson);

                            // Only after success, move the file to 'processed'
                            var currentDirectory = Path.GetDirectoryName(filePath)!;
                            var completedDir = Path.Combine(currentDirectory, "processed");

                            if (!Directory.Exists(completedDir))
                                Directory.CreateDirectory(completedDir);
            
                            var completedFilePath = Path.Combine(completedDir, fileName);
                            File.Move(filePath, completedFilePath);
                            _logger.LogInformation("📂 Moved file to 'processed': {CompletedFilePath}", completedFilePath);

                            // Save to database
                            var dbScope = _services.CreateScope();
                            var audioService = dbScope.ServiceProvider.GetRequiredService<IAudioRepository>();
                            _logger.LogInformation($"Deserialized result => GuideId: {result?.id} {result.message} {completedDir}");

                            var audioFile = new AudioFile()
                            {
                                AudioFilePath = completedFilePath,
                                FileName = fileName, 
                                ProcessedFileId = result.id,
                                Status = "InQueue",
                                ConvertedAt = DateTime.UtcNow,
                                Transcription = ""
                            };

                            await audioService.AddAsync(audioFile);
                            _logger.LogInformation("💾 Audio file info saved to database for: {FileName}", audioFile.FileName);
                        }
                        else
                        {
                            _logger.LogError("❌ Upload failed for {FileName}. Status Code: {StatusCode}", fileName, response.StatusCode);
                            queue.Enqueue(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to process: {FilePath}", filePath);
                    }
                }
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
}
