#region Copyright

/*
 * File: VoiceOggRecognizer.cs
 * Author: denisosipenko
 * Created: 2024-07-03
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

using Concentus;
using Concentus.Oggfile;

namespace VoiceBot.Services;

public class VoiceOggRecognizer : IVoiceRecognizer
{
    private readonly VoiceWaveRecognizer _waveRecognizer;

    public VoiceOggRecognizer(VoiceWaveRecognizer waveRecognizer)
    {
        _waveRecognizer = waveRecognizer;
    }

    public async Task<string> GetText(MemoryStream stream, VoiceMeta meta)
    {
        if (meta.Type is not SourceVoiceType.Ogg)
            throw new ArgumentException($"Не могу распознать тип {meta.Type.ToString()}");

        stream.Seek(0, SeekOrigin.Begin);
        using IOpusDecoder? decoder = OpusCodecFactory.CreateDecoder(48000, 1);
        OpusOggReadStream oggIn = new(decoder, stream);
        await using MemoryStream waveStream = ReadWaveStream(oggIn);

        var waveMeta = (VoiceMeta) meta.Clone();
        waveMeta.Type = SourceVoiceType.Wave;
        return await _waveRecognizer.GetText(waveStream, waveMeta);
    }

    private MemoryStream ReadWaveStream(OpusOggReadStream stream)
    {
        var tempStream = new MemoryStream();
        while (stream.HasNextPacket)
        {
            short[] packet = stream.DecodeNextPacket();
            if (packet != null)
                for (int i = 0; i < packet.Length; i++)
                {
                    byte[] bytes = BitConverter.GetBytes(packet[i]);
                    tempStream.Write(bytes, 0, bytes.Length);
                }
        }

        tempStream.Seek(0, SeekOrigin.Begin);
        return tempStream;
    }
}