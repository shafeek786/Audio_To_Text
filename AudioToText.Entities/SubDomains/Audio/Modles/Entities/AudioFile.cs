using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AudioToText.Entities.SubDomains.Audio.Modles
{
    public class AudioFile
    {
        [Key]
        public Guid ProcessedFileGuid { get; set; }   // Primary Key

        public string AudioFilePath { get; set; }
        public string FileName { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConvertedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Transcription { get; set; }
        public string? SrtText { get; set; }
        public int RetryCount { get; set; } = 0;

        public ICollection<AudioFileSrtSegment> AudioFileSrtSegments { get; set; } = new List<AudioFileSrtSegment>();
    }

}