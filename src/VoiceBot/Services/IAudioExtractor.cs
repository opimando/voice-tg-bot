#region Copyright

/*
 * File: IAudioExecutor.cs
 * Author: denisosipenko
 * Created: 2024-07-02
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

namespace VoiceBot.Services;

public interface IAudioExtractor
{
    Task<MemoryStream> GetAudio(MemoryStream videoStream);
}