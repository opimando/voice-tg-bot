#region Copyright

/*
 * File: VoiceCommandHandler.cs
 * Author: denisosipenko
 * Created: 2024-07-02
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

using TgBotFramework.Core;
using VoiceBot.Services;

namespace VoiceBot.Commands;

[TelegramState(true)]
public class VoiceCommandHandler : BaseChatState
{
    private readonly IVoiceRecognizer _voiceRecognizer;
    private readonly IFileProvider _fileProvider;
    private readonly IAudioExtractor _audioExtractor;

    public VoiceCommandHandler(
        IVoiceRecognizer voiceRecognizer,
        IFileProvider fileProvider,
        IAudioExtractor audioExtractor)
    {
        _voiceRecognizer = voiceRecognizer;
        _fileProvider = fileProvider;
        _audioExtractor = audioExtractor;
    }

    protected override async Task<IStateInfo> InternalProcessMessage(Message receivedMessage)
    {
        (VoiceContent? voice, VideoNoteContent? video) = GetVideo(receivedMessage);

        await ProcessAndReplay(Messenger, receivedMessage.Id, receivedMessage.ChatId, voice, video);

        return new StateInfo(this);
    }

    private (VoiceContent? Voice, VideoNoteContent? Video) GetVideo(Message message)
    {
        VoiceContent? voice = null;
        VideoNoteContent? video = null;

        switch (message.Content)
        {
            case VoiceContent vContent:
                voice = vContent;
                break;
            case VideoNoteContent vdContent:
                video = vdContent;
                break;
        }

        return (voice, video);
    }

    private async Task ProcessAndReplay(IMessenger messenger, MessageId messageId, ChatId chatId, VoiceContent? voice,
        VideoNoteContent? video)
    {
        MessageId? infoMessageId = null;

        try
        {
            infoMessageId = await messenger.Send(chatId,
                new SendInfo(new TextContent("Пробуем расшифровать...")) {HideNotification = true}
            );

            string? text = null;

            if (voice != null)
                text = await ProcessAudioMessage(voice, messenger, chatId);
            else if (video != null)
                text = await ProcessVideoMessage(video, messenger, chatId);

            if (string.IsNullOrWhiteSpace(text))
            {
                await messenger.Reply(chatId, messageId, "Сообщение пустое или я ничего не смог понять :(");
                return;
            }

            await messenger.Reply(chatId, messageId, text);
        }
        catch (Exception ex)
        {
            await messenger.Send(chatId, "Произошла ошибка :(");
            EventsBus.Publish(new ErrorEvent(ex, "Ошибка при парсинге голоса"));
        }
        finally
        {
            if (infoMessageId != null)
                await messenger.Delete(chatId, infoMessageId);
        }
    }

    private async Task<string> ProcessAudioMessage(VoiceContent voice, IMessenger messenger, ChatId chatId)
    {
        using var stream = new MemoryStream();

        if (voice.Data == null)
        {
            await using MemoryStream content = await _fileProvider.DownloadFile(voice.FileId);
            content.Seek(0, SeekOrigin.Begin);
            await content.CopyToAsync(stream);
        }
        else
        {
            await voice.Data.CopyToAsync(stream);
        }

        string text = await _voiceRecognizer.GetText(stream, new VoiceMeta {Type = SourceVoiceType.Ogg});

        voice.Dispose();
        return text;
    }

    private async Task<string> ProcessVideoMessage(VideoNoteContent video, IMessenger messenger, ChatId chatId)
    {
        using var stream = new MemoryStream();

        if (video.Data == null)
        {
            await using MemoryStream content = await _fileProvider.DownloadFile(video.FileId);
            content.Position = 0;
            await content.CopyToAsync(stream);
        }
        else
        {
            await video.Data.CopyToAsync(stream);
        }

        using MemoryStream audio = await _audioExtractor.GetAudio(stream);
        string text = await _voiceRecognizer.GetText(audio, new VoiceMeta {Type = SourceVoiceType.Wave});

        video.Dispose();
        return text;
    }

    [CustomStaticAccessFunction]
    public static bool CanProcess(Message message)
    {
        return true;
    }
}