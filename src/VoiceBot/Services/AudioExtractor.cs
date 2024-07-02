#region Copyright

/*
 * File: AudioExecutor.cs
 * Author: denisosipenko
 * Created: 2024-07-02
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

using NAudio.Wave;

namespace VoiceBot.Services;

/// <summary>
/// Извлекает аудио из видео. Как оказалось только в windows.
/// </summary>
public class AudioExtractor : IAudioExtractor
{
    public async Task<MemoryStream> GetAudio(MemoryStream videoStream)
    {
        MemoryStream audioStream = new();

        await using StreamMediaFoundationReader mediaReader = new(videoStream);

        if (!mediaReader.CanRead)
            throw new InvalidCastException("Невозможно получить аудио дорожку");

        mediaReader.Seek(0, SeekOrigin.Begin);

        WaveFileWriter.WriteWavFileToStream(audioStream, mediaReader);
        audioStream.Seek(0, SeekOrigin.Begin);

        return audioStream;
    }
}