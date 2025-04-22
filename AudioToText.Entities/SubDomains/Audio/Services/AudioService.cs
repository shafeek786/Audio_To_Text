using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using AudioToText.Entities.SubDomains.Audio.Interface;
using AudioToText.Entities.SubDomains.Audio.Modles;
using AudioToText.Entities.SubDomains.Audio.Modles.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AudioToText.Entities.SubDomains.Audio.Services
{
    public class AudioService : IAudioService
    {
        private readonly IAudioRepository _audioRepository;
        private readonly ILogger<AudioService> _logger;
        private readonly string _pythonPath;
        private readonly IHttpClientFactory _httpClientFactory;

        public AudioService(
            IAudioRepository audioRepository,
            ILogger<AudioService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _audioRepository = audioRepository;
            _logger = logger;
            _pythonPath = configuration["PythonSettings:PythonExePath"];
            _httpClientFactory = httpClientFactory;

            if (string.IsNullOrWhiteSpace(_pythonPath))
            {
                throw new InvalidOperationException("Python executable path is not configured.");
            }
        }

        public async Task<AudioFileDto> ProcessAudioAsync(IFormFile file, string rootPath)
        {
            var tempFilePath = Path.Combine(rootPath, file.FileName);
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            await using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream).ConfigureAwait(false);
            }

            return await SaveAndTranscribeAsync(tempFilePath, file.FileName).ConfigureAwait(false);
        }

        public async Task<Guid> SaveAudioAndGenerateGuideIdAsync(IFormFile file, string rootPath)
        {
            var tempFilePath = Path.Combine(rootPath, file.FileName);
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            await using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream).ConfigureAwait(false);
            }

            var guideId = Guid.NewGuid();

            // var audio = new AudioFile
            // {
            //     FileName = file.FileName,
            //     Transcription = string.Empty,
            //     AudioFilePath = tempFilePath,
            //     ProcessedFileGuid = guideId
            // };
            //
            // await _audioRepository.AddAsync(audio).ConfigureAwait(false);
            return guideId;
        }

        public async Task<AudioFileDto> ProcessAudioWithCallbackAsync(IFormFile file, string rootPath, string callbackUrl, string guideId)
        {
            var tempFilePath = Path.Combine(rootPath, file.FileName);
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            await using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream).ConfigureAwait(false);
            }

            var result = await SaveAndTranscribeAsync(tempFilePath, file.FileName).ConfigureAwait(false);

            var payload = new { GuideId = guideId, Transcription = result.Transcription };

            try
            {
                using var client = _httpClientFactory.CreateClient();
                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(callbackUrl, content).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Callback to {CallbackUrl} failed with status code {StatusCode}", callbackUrl, response.StatusCode);
                }
                else
                {
                    _logger.LogInformation("Callback to {CallbackUrl} succeeded.", callbackUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling the callback URL: {CallbackUrl}", callbackUrl);
            }

            return result;
        }

        public async Task<AudioFileDto> ProcessExistingFileAsync(string existingFilePath)
        {
            if (!File.Exists(existingFilePath))
            {
                var msg = $"Audio file not found at: {existingFilePath}";
                _logger.LogError(msg);
                throw new FileNotFoundException(msg, existingFilePath);
            }

            var fileName = Path.GetFileName(existingFilePath);
            return await SaveAndTranscribeAsync(existingFilePath, fileName).ConfigureAwait(false);
        }

        private async Task<AudioFileDto> SaveAndTranscribeAsync(string filePath, string fileName)
        {
            var transcription = await TranscribeWithApi(filePath).ConfigureAwait(false);

            var audio = new AudioFile
            {
                FileName = fileName,
                Transcription = transcription,
                AudioFilePath = filePath
            };

            var saved = await _audioRepository.AddAsync(audio).ConfigureAwait(false);
            return new AudioFileDto
            {
                FileName = saved.FileName,
                Transcription = saved.Transcription,
                AudioFilePath = saved.AudioFilePath
            };
        }

        public List<AudioFileDto> GetAll()
        {
            return _audioRepository.GetAll()
                .Select(x => new AudioFileDto
                {
                    FileName = x.FileName,
                    Transcription = x.Transcription,
                    AudioFilePath = x.AudioFilePath,
                    SrtText = x.SrtText,
                })
                .ToList();
        }

        private async Task<string> TranscribeWithApi(string filePath)
        {
            if (!File.Exists(filePath))
            {
                var msg = $"Audio file not found for transcription at: {filePath}";
                _logger.LogError(msg);
                throw new FileNotFoundException(msg, filePath);
            }

            using var client = _httpClientFactory.CreateClient();
            using var multipartContent = new MultipartFormDataContent();

            var fileContent = new StreamContent(File.OpenRead(filePath))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("audio/mpeg") }
            };

            multipartContent.Add(fileContent, "audio", Path.GetFileName(filePath));

            var response = await client.PostAsync("http://localhost:5000/transcribe", multipartContent).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException($"Transcription API failed: {responseString}");
            }

            return responseString;
        }
        
        
        private async Task<string> TranscribeFileOnlyAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                var msg = $"Audio file not found for transcription at: {filePath}";
                _logger.LogError(msg);
                throw new FileNotFoundException(msg, filePath);
            }

            return await TranscribeWithApi(filePath).ConfigureAwait(false);
        }

        
        
        
        public async Task ProcessAndNotifyAsync(Guid guideId, string filePath, string callbackUrl)
        {
            try
            {
                
                // Process the file (transcribe or other operations)
                var result = await TranscribeFileOnlyAsync(filePath);
                var extracted = JsonSerializer.Deserialize<TranscriptionResult>(result);
                
                _logger.LogInformation($"resulr: {extracted} srt: {extracted.srt}");
                // Create the payload to send to the callback URL
                var payload = new
                {
                    Guid  = guideId,
                    Transcription = extracted.text.Trim(),
                    Srt = extracted?.srt?.Trim(),
                    ConvertedAt = DateTime.UtcNow
                };
               
                using var client = _httpClientFactory.CreateClient();
                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                // Send the result to the callback URL
                var response = await client.PostAsync(callbackUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Callback to {CallbackUrl} succeeded.", callbackUrl);
                }
                else
                {
                    _logger.LogError("Callback to {CallbackUrl} failed with status code {StatusCode}", callbackUrl, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing and notifying.{Message}", ex.Message);
            }
        }

        
    }
}
