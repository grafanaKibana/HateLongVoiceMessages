#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001
namespace HateLongVoiceMessages;

using HateLongVoiceMessages.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public static class MessageHandlers
{
    public static async Task OnUpdate(TelegramBotClient bot, IAudioToTextService audioToTextService, Kernel kernel, Update update)
    {
        if (update!.Type != UpdateType.Message)
        {
            return;
        }

        var message = update.Message;

        if (message is not {Type: MessageType.Voice} || message.Voice == null)
        {
            return;
        }

        var aiResponse = await audioToTextService.GetTextContentAsync(
            await ExtractAudioContentFromMessage(message, bot),
            new OpenAIAudioToTextExecutionSettings {Language = SummarizeMessagePlugin.Language,},
            kernel);

        var summarized = await kernel.Plugins
            .GetFunction(nameof(SummarizeMessagePlugin), "SummarizeMessageTranscript")
            .InvokeAsync(kernel, new KernelArguments
            {
                { "language", SummarizeMessagePlugin.Language },
                { "input", aiResponse.ToString() },
            });

        await bot.SendMessage(message.Chat.Id, summarized.ToString(), replyParameters: message.MessageId);
    }

    private static async Task<AudioContent> ExtractAudioContentFromMessage(Message message, TelegramBotClient bot)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Voice);
        ArgumentNullException.ThrowIfNull(message.Voice.FileId);

        using var memoryStream = new MemoryStream();

        var voiceFile = await bot.GetFile(message.Voice.FileId);
        await bot.DownloadFile(voiceFile.FilePath ?? string.Empty, memoryStream);

        memoryStream.Seek(0, SeekOrigin.Begin);

        return new AudioContent(memoryStream.ToArray(), message.Voice?.MimeType);
    }
}