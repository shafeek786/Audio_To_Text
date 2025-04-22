using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AudioToText.Entities.SubDomains.Audio.Modles
{
    public class AudioFile
    {
        [Key]
        public long AudioFileId { get; set; }

        public string AudioFilePath { get; set; }

        public string FileName { get; set; }

        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;  

        public DateTime? ConvertedAt { get; set; }  

        public string Status { get; set; } = "Pending";

        public Guid? ProcessedFileGuid { get; set; }

        public string? Transcription { get; set; }  

        public string? SrtText { get; set; }       

        public int RetryCount { get; set; } = 0;
    }
}