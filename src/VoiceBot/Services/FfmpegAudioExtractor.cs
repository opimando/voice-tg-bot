#region Copyright

/*
 * File: FfmpegAudioExtractor.cs
 * Author: denisosipenko
 * Created: 2024-07-03
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

using Xabe.FFmpeg;

namespace VoiceBot.Services;

public class FfmpegAudioExtractor : IAudioExtractor
{
    public async Task<MemoryStream> GetAudio(MemoryStream videoStream)
    {
        string tempInputFileName = Guid.NewGuid() + ".mp4";
        string tempOutputFileName = Guid.NewGuid() + ".wav";

        try
        {
            videoStream.Seek(0, SeekOrigin.Begin);

            await File.WriteAllBytesAsync(tempInputFileName, videoStream.ToArray());

            IMediaInfo? mediaInfo = await FFmpeg.GetMediaInfo(tempInputFileName);
            IAudioStream? audioStream = mediaInfo.AudioStreams.FirstOrDefault();

            if (audioStream == null) throw new Exception("No audio stream found in the input video.");

            IConversion? conversion = FFmpeg.Conversions.New()
                .AddStream(audioStream)
                .SetOutput(tempOutputFileName)
                .SetOutputFormat(Format.wav)
                .SetAudioBitrate(48000);

            await conversion.Start();

            byte[] outputAudioBytes = await File.ReadAllBytesAsync(tempOutputFileName);
            var ret = new MemoryStream(outputAudioBytes);
            ret.Seek(0, SeekOrigin.Begin);
            return ret;
        }
        finally
        {
            if (File.Exists(tempInputFileName)) File.Delete(tempInputFileName);
            if (File.Exists(tempOutputFileName)) File.Delete(tempOutputFileName);
        }
    }
}