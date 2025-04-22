namespace AudioToText.Entities.SubDomains.Audio.Modles.DTO;

public class AudioFileDto
{
    public string FileName{ get; set; }
    
    public string Transcription { get; set; } 
    
    public string  AudioFilePath { get; set; }
    
    public string? SrtText { get; set; }  
}