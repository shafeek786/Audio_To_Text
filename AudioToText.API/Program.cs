using AudioToText.Entities.DataBaseContext;

using AudioToText.Entities.SubDomains.Audio.Interface;
using AudioToText.Entities.SubDomains.Audio.Repository;
using AudioToText.Entities.SubDomains.Audio.Services;
using AudioToText.Entities.SubDomains.AudioWatcher.Interface;
using AudioToText.Entities.SubDomains.AudioWatcher.Model;
using AudioToText.Entities.SubDomains.AudioWatcher.Services;
using AudioToText.Entities.SubDomains.Callback.Interface;
using AudioToText.Entities.SubDomains.Callback.Repository;
using AudioToText.Entities.SubDomains.Callback.Services;
using AudioToText.Entities.SubDomains.FileExplorer.Interface;
using AudioToText.Entities.SubDomains.FileExplorer.Services;
using AudioToText.Entities.SubDomains.Queue.Interface;
using AudioToText.Entities.SubDomains.Queue.Services;
using AudioToText.Entities.TranscriptionServiceOptions;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddScoped<IAudioRepository, AudioREpository>();
builder.Services.AddScoped<IAudioService, AudioService>();


builder.Services.AddScoped<AudioDbContext, AudioDbContext>();

builder.Services.AddSingleton<IAudioQueueService, AudioQueueService>();
builder.Services.AddHostedService<AudioQueueProcessorService>();

builder.Services.AddHostedService<HybridAudioWatcherService>();
builder.Services.AddSingleton<IFileDetectionStrategy, FileSystemWatcherStrategy>();
builder.Services.AddSingleton<IFileDetectionStrategy, PollingDetectionStrategy>();
builder.Services.AddSingleton<IFileProcessorService, FileProcessorService>();

builder.Services.AddScoped<ICallbackService, CallbackService>();
builder.Services.AddScoped<CallbackRepository>();

builder.Services.AddScoped<IFileExplorerService, FileExplorerService>();

builder.Services.AddScoped<ICompletedFileExplorerService, CompletedFileExplorerService>();
builder.Services.AddScoped<IAudioFileService, AudioFileService>();

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();


builder.Services.Configure<AudioWatcherOptions>(
    builder.Configuration.GetSection("AudioWatcher")
    );
builder.Services.Configure<TranscriptionServiceOptions>(
    builder.Configuration.GetSection("TranscriptionService")
    );
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();



app.Run();

