namespace VoiceBot.Models;

public class AudioStream : BaseBinaryContent, IAudioContent
{
    private readonly SourceVoiceType _type;

    public AudioStream(MemoryStream stream, SourceVoiceType type) : base(stream)
    {
        _type = type;
    }

    public SourceVoiceType GetSourceType()
    {
        return _type;
    }
}