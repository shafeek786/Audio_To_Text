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
using AudioToText.Entities.SubDomains.Queue.Model.DTO;
using AudioToText.Entities.SubDomains.Queue.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services for Dependency Injection
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

builder.Services.Configure<AudioServiceSettingsDTO>(
    builder.Configuration.GetSection("AudioServiceSettings"));

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration for AudioWatcherOptions (from appsettings.json)
builder.Services.Configure<AudioWatcherOptions>(
    builder.Configuration.GetSection("AudioWatcher")
);

builder.WebHost.UseUrls("http://0.0.0.0:8078");




// Set up CORS policy to allow specific origins (Flutter web in this case)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterWeb", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5000", // Local dev environment
                "http://127.0.0.1:5000", // Local dev environment
                "http://192.168.29.180:5000" // Flutter web on external network IP
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Apply the CORS policy globally
app.UseCors("AllowFlutterWeb");

// Configure the HTTP request pipeline in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map Controllers for the application
app.MapControllers();

// Run the application
app.Run();
