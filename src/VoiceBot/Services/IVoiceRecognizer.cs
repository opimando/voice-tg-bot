#region Copyright

/*
 * File: IVoiceRecognizer.cs
 * Author: denisosipenko
 * Created: 2024-07-02
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

namespace VoiceBot.Services;

public interface IVoiceRecognizer
{
    Task<string> GetText(MemoryStream stream, VoiceMeta meta);
}

public class VoiceMeta : ICloneable
{
    public SourceVoiceType Type { get; set; }

    public object Clone()
    {
        return new VoiceMeta
        {
            Type = Type
        };
    }
}

public enum SourceVoiceType
{
    Ogg,
    Wave
}