#region Copyright

/*
 * File: IVoiceRecognizer.cs
 * Author: denisosipenko
 * Created: 2024-07-02
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

using VoiceBot.Models;

namespace VoiceBot.Services;

public interface IVoiceRecognizer
{
    Task<string> GetText(IAudioContent stream);
}