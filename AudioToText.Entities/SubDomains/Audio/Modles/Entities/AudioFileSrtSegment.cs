using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AudioToText.Entities.SubDomains.Audio.Modles;

public class AudioFileSrtSegment
{
    [Key]
    public long SegmentId { get; set; }

    [Required]
    public Guid ProcessedFileGuid { get; set; }

    public int SegmentOrder { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    [Required]
    public string TranscriptText { get; set; }

    [ForeignKey(nameof(ProcessedFileGuid))]
    public AudioFile AudioFile { get; set; }
}
