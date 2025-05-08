#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0050
namespace HateLongVoiceMessages;

using HateLongVoiceMessages.Configs;
using HateLongVoiceMessages.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Telegram.Bot;

public static class DependencyInjection
{
    public static IServiceCollection AddTelegram(this IServiceCollection services, IConfiguration configuration)
    {

        var telegramConfig = configuration
            .GetRequiredSection(nameof(Telegram))
            .Get<Telegram>();

        services.ConfigureTelegramBot<Microsoft.AspNetCore.Http.Json.JsonOptions>(opt => opt.SerializerOptions);
        services.AddHttpClient("tgwebhook")
            .RemoveAllLoggers()
            .AddTypedClient(httpClient => new TelegramBotClient(telegramConfig.ApiKey, httpClient));


        /*
        services.AddSingleton<ITelegramBotClient>(_ =>
            telegramConfig != null
                ? new TelegramBotClient(new TelegramBotClientOptions(telegramConfig.ApiKey))
                : throw new ArgumentNullException(nameof(telegramConfig)));
                */

        return services;
    }

    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        var openAiOptions = configuration
            .GetRequiredSection(nameof(AzureOpenAI))
            .Get<AzureOpenAI>();

        var kernelBuilder = services.AddKernel();

        kernelBuilder.Services
            .AddAzureOpenAIChatCompletion("gpt-4o-mini", openAiOptions.Endpoint, openAiOptions.ApiKey)
            .AddAzureOpenAIAudioToText("whisper", openAiOptions.Endpoint, openAiOptions.ApiKey);

        kernelBuilder.Plugins.AddFromType<SummarizeMessagePlugin>();

        return services;
    }
}