namespace VoiceBot.Models;

public interface IAudioContent : IMediaContent
{
    SourceVoiceType GetSourceType();
}