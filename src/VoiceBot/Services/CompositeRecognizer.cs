#region Copyright

/*
 * File: CompositeRecognizer.cs
 * Author: denisosipenko
 * Created: 2024-07-03
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

using VoiceBot.Models;

namespace VoiceBot.Services;

public class CompositeRecognizer : IVoiceRecognizer, IDisposable
{
    private readonly VoiceWaveRecognizer _waveRecognizer = new();
    private readonly VoiceOggRecognizer _oggRecognizer;

    public CompositeRecognizer()
    {
        _oggRecognizer = new VoiceOggRecognizer(_waveRecognizer);
    }

    public Task<string> GetText(IAudioContent stream)
    {
        return stream.GetSourceType() switch
        {
            SourceVoiceType.Ogg => _oggRecognizer.GetText(stream),
            SourceVoiceType.Wave => _waveRecognizer.GetText(stream),
            _ => throw new ArgumentOutOfRangeException($"Не умею распознавать аудио типа {stream.GetSourceType().ToString()}")
        };
    }

    public void Dispose()
    {
        _waveRecognizer.Dispose();
    }
}