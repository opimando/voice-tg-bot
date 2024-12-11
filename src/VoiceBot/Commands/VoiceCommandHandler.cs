#region Copyright

/*
 * File: VoiceCommandHandler.cs
 * Author: denisosipenko
 * Created: 2024-07-02
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

using TgBotFramework.Core;
using VoiceBot.Models;
using VoiceBot.Services;
using VideoContent = VoiceBot.Models.VideoContent;

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
        BaseFileContent? content = GetContentWithAudio(receivedMessage);
        if (content == null)
            return new StateInfo(this);

        await ProcessAndReplay(Messenger, receivedMessage.Id, receivedMessage.ChatId, content);

        return new StateInfo(this);
    }

    private BaseFileContent? GetContentWithAudio(Message message)
    {
        return message.Content switch
        {
            VoiceContent voice => voice,
            VideoNoteContent video => video,
            AudioContent audio => audio,
            _ => null
        };
    }

    private async Task ProcessAndReplay(IMessenger messenger, MessageId messageId, ChatId chatId,
        BaseFileContent content)
    {
        MessageId? infoMessageId = null;

        try
        {
            infoMessageId = await messenger.Send(chatId,
                new SendInfo(new TextContent("Пробуем расшифровать...")) {HideNotification = true}
            );

            string? text = null;

            if (content is AudioContent audio)
                text = await ProcessAudioMessage(audio, messenger, chatId);
            else if (content is VideoNoteContent video)
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

    private async Task<string> ProcessAudioMessage(AudioContent audio, IMessenger messenger, ChatId chatId)
    {
        using var stream = new MemoryStream();

        if (audio.Data == null)
        {
            await using MemoryStream content = await _fileProvider.DownloadFile(audio.FileId);
            content.Seek(0, SeekOrigin.Begin);
            await content.CopyToAsync(stream);
        }
        else
        {
            await audio.Data.CopyToAsync(stream);
        }

        string text = await _voiceRecognizer.GetText(new AudioStream(stream, SourceVoiceType.Ogg));

        audio.Dispose();
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

        IAudioContent audio = await _audioExtractor.GetAudio(new VideoContent(stream));
        string text = await _voiceRecognizer.GetText(audio);

        video.Dispose();
        return text;
    }

    [CustomStaticAccessFunction]
    public static bool CanProcess(Message message)
    {
        return true;
    }
}