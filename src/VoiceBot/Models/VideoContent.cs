namespace VoiceBot.Models;

public class VideoContent : BaseBinaryContent, IVideoContent
{
    public VideoContent(MemoryStream stream) : base(stream)
    {
    }
}