#region Copyright

/*
 * File: CompositeRecognizer.cs
 * Author: denisosipenko
 * Created: 2024-07-03
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

namespace VoiceBot.Services;

public class CompositeRecognizer : IVoiceRecognizer, IDisposable
{
    private readonly VoiceWaveRecognizer _waveRecognizer = new();
    private readonly VoiceOggRecognizer _oggRecognizer;

    public CompositeRecognizer()
    {
        _oggRecognizer = new VoiceOggRecognizer(_waveRecognizer);
    }

    public Task<string> GetText(MemoryStream stream, VoiceMeta meta)
    {
        return meta.Type switch
        {
            SourceVoiceType.Ogg => _oggRecognizer.GetText(stream, meta),
            SourceVoiceType.Wave => _waveRecognizer.GetText(stream, meta),
            _ => throw new ArgumentOutOfRangeException($"Не умею распознавать аудио типа {meta.Type.ToString()}")
        };
    }

    public void Dispose()
    {
        _waveRecognizer.Dispose();
    }
}