namespace VoiceBot.Models;

public abstract class BaseBinaryContent : IMediaContent
{
    protected readonly byte[] Data;

    public BaseBinaryContent(MemoryStream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        byte[] buffer;

        try
        {
            buffer = stream.GetBuffer();
        }
        catch (UnauthorizedAccessException)
        {
            buffer = stream.ToArray();
        }

        Data = new byte[stream.Length];
        Array.Copy(buffer, Data, stream.Length);
    }

    public virtual MemoryStream Get()
    {
        return new MemoryStream(Data);
    }
}