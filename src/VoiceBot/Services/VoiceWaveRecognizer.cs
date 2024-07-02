#region Copyright

/*
 * File: VoiceWaveRecognizer.cs
 * Author: denisosipenko
 * Created: 2024-07-03
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

using NAudio.Wave;
using Newtonsoft.Json;
using Vosk;

namespace VoiceBot.Services;

public class VoiceWaveRecognizer : IVoiceRecognizer, IDisposable
{
    private readonly Model _model = new("model");

    public async Task<string> GetText(MemoryStream stream, VoiceMeta meta)
    {
        if (meta.Type is not SourceVoiceType.Wave)
            throw new ArgumentException($"Не могу распознать тип {meta.Type.ToString()}");

        stream.Seek(0, SeekOrigin.Begin);
        await using RawSourceWaveStream waveStream = new(stream, new WaveFormat(48000, 1));
        using var rec = new VoskRecognizer(_model, 48000.0f);

        byte[] buffer = new byte[4096];
        int bytesRead;

        while ((bytesRead = await waveStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            rec.AcceptWaveform(buffer, bytesRead);

        string? result = rec.FinalResult();
        if (string.IsNullOrWhiteSpace(result))
            return string.Empty;

        var item = JsonConvert.DeserializeObject<VoskResult>(result);
        return item?.text ?? string.Empty;
    }

    public void Dispose()
    {
        _model.Dispose();
    }

    private class VoskResult
    {
        public string text { get; set; }
    }
}