using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Net.Http.Headers;
using AudioToText.Entities.SubDomains.Audio.Interface;
using AudioToText.Entities.SubDomains.Queue.Interface;
using AudioToText.Entities.SubDomains.Audio.Modles;
using AudioToText.Entities.SubDomains.Audio.Modles.DTO;
using AudioToText.Entities.SubDomains.Callback.Model.DTO;

namespace AudioToText.Entities.SubDomains.Audio.Services
{
    public class AudioQueueProcessorService : BackgroundService
    {
        private readonly ILogger<AudioQueueProcessorService> _logger;
        private readonly IServiceProvider _services;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string WebhookUrl = "https://localhost:44365/api/Callback/receive";
        private const string TranscriptionType = "string"; // Update with your actual type if needed
        private const string TranscribeApiUrl = "https://localhost:44386/api/Transcribe";

        public AudioQueueProcessorService(
            ILogger<AudioQueueProcessorService> logger,
            IServiceProvider services,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _services = services;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🎯 AudioQueueProcessorService started.");

            using var scope = _services.CreateScope();
            var queue = scope.ServiceProvider.GetRequiredService<IAudioQueueService>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var filePath = await queue.DequeueAsync(stoppingToken);

                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    await Task.Delay(1000, stoppingToken); // Avoid tight loop
                    continue;
                }

                try
                {
                    _logger.LogInformation("Processing file: {FilePath}", filePath);

                    var client = _httpClientFactory.CreateClient();

                    using var formContent = await BuildFormContentAsync(filePath);
                    client.DefaultRequestHeaders.Add("TenantId", "1");

                    var response = await client.PostAsync(TranscribeApiUrl, formContent, stoppingToken);

                    _logger.LogInformation($"Transcribe response@@@@@@@@@@@@@@@@@@@@@@@@@@: {response}");
                    Console.WriteLine($"Transcribe response################: {response}");
                    if (response.IsSuccessStatusCode)
                    {
                        var resultJson = await response.Content.ReadAsStringAsync(stoppingToken);
                        _logger.LogInformation($"Transcribe response========================>: {resultJson}");
                        var resultresponse = JsonSerializer.Deserialize<TranscriptionResponse>(resultJson);
                        _logger.LogInformation($"Transcribe response###############: {resultresponse}");
                        var result = new UploadResponse
                        {
                            guideId = resultresponse.id,
                            message = "File received. Processing started"
                        };
                            
                        _logger.LogInformation($"{result.guideId}-------------------------------------->");
                        

                        await HandleSuccessAsync(filePath, result);
                    }
                    else
                    {
                        _logger.LogError("❌ Upload failed for {FileName}. Status Code: {StatusCode}", Path.GetFileName(filePath), response.StatusCode);
                        queue.Enqueue(filePath); // Requeue for retry
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Exception while processing {FilePath}", filePath);
                }
            }
        }

        private async Task<MultipartFormDataContent> BuildFormContentAsync(string filePath)
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var memoryStream = new MemoryStream(fileBytes);
            var fileName = Path.GetFileName(filePath);

            var formContent = new MultipartFormDataContent();

            var fileContent = new StreamContent(memoryStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(Path.GetExtension(filePath)));

            formContent.Add(fileContent, "audioFile", fileName);
            formContent.Add(new StringContent(WebhookUrl), "webhookUrl");
            formContent.Add(new StringContent(TranscriptionType), "TranscriptionType");

            return formContent;
        }

        private async Task HandleSuccessAsync(string originalFilePath, UploadResponse? result)
        {
            var currentDirectory = Path.GetDirectoryName(originalFilePath)!;
            var processedDirectory = Path.Combine(currentDirectory, "processed");

            if (!Directory.Exists(processedDirectory))
                Directory.CreateDirectory(processedDirectory);

            var completedFilePath = Path.Combine(processedDirectory, Path.GetFileName(originalFilePath));
            File.Move(originalFilePath, completedFilePath);

            _logger.LogInformation("📂 Moved file to 'processed' folder: {CompletedFilePath}", completedFilePath);

            using var dbScope = _services.CreateScope();
            var audioRepository = dbScope.ServiceProvider.GetRequiredService<IAudioRepository>();

            var audioFile = new AudioFile
            {
                AudioFilePath = completedFilePath,
                FileName = Path.GetFileName(originalFilePath),
                ProcessedFileGuid = result?.guideId,
                Status = "Processed",
                ConvertedAt = DateTime.UtcNow,
                Transcription = string.Empty
            };

            await audioRepository.AddAsync(audioFile);
            _logger.LogInformation("💾 Saved audio file metadata to database: {FileName}", audioFile.FileName);
        }

        private string GetContentType(string extension) =>
            extension.ToLowerInvariant() switch
            {
                ".wav" => "audio/wav",
                ".mp3" => "audio/mpeg",
                ".m4a" => "audio/mp4",
                _ => "application/octet-stream"
            };
    }
}
