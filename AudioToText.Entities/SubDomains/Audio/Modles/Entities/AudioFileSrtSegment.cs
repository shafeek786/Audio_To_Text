using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AudioToText.Entities.SubDomains.Audio.Modles;

public class AudioFileSrtSegment
{
    [Key]
    public long SegmentId { get; set; }

    [Required]
    public long ProcessedFileId { get; set; }

    public int SegmentOrder { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    [Required]
    public string TranscriptText { get; set; }

    [ForeignKey(nameof(ProcessedFileId))]
    public AudioFile AudioFile { get; set; }
}
