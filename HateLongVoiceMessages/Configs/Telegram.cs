namespace HateLongVoiceMessages.Configs;

public record Telegram
{
    public Telegram() { }
    public Telegram(string apiKey)
    {
        this.ApiKey = apiKey;
    }
    public required string ApiKey { get; init; }
}