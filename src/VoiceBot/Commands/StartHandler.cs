#region Copyright

/*
 * File: StartHandler.cs
 * Author: denisosipenko
 * Created: 2024-07-02
 * Copyright © 2024 Denis Osipenko
 */

#endregion Copyright

using TgBotFramework.Core;

namespace VoiceBot.Commands;

[TelegramState("/start", "/help")]
public class StartHandler : BaseChatState
{
    private readonly IChatStateFactory _stateFactory;

    public override StatePriority Priority => StatePriority.Priority;

    public StartHandler(IChatStateFactory stateFactory)
    {
        _stateFactory = stateFactory;
    }

    protected override async Task<IStateInfo> InternalProcessMessage(Message receivedMessage)
    {
        await Messenger.Send(receivedMessage.ChatId, "Перешли голосовое сообщение и я попробую его превратить в текст");
        var next = await _stateFactory.CreateState<VoiceCommandHandler>();
        return new StateInfo(next, ExecutionType.RightNow);
    }
}