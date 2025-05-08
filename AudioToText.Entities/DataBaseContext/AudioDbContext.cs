using AudioToText.Entities.SubDomains.Audio.Modles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AudioToText.Entities.DataBaseContext;

public class AudioDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public AudioDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public DbSet<AudioFile> AudioFiles { get; set; }
    
    public DbSet<AudioFileSrtSegment> AudioFileSrtSegments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string? connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
                var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "user_authentication";
                var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "apple";
                var dbPass = Environment.GetEnvironmentVariable("DB_PASS") ?? "12345";
                var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";

                connectionString = $"Server={dbHost};Database={dbName};Username={dbUser};Password={dbPass};Port={dbPort};";
            }

            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}
