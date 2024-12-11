#region Copyright

/*
 * File: IAudioExecutor.cs
 * Author: denisosipenko
 * Created: 2024-07-02
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

using VoiceBot.Models;

namespace VoiceBot.Services;

public interface IAudioExtractor
{
    Task<IAudioContent> GetAudio(IVideoContent video);
}