namespace HateLongVoiceMessages.Configs;

public record AzureOpenAI
{
    public AzureOpenAI()
    {

    }
    public AzureOpenAI(string Endpoint, string ApiKey)
    {
        this.Endpoint = Endpoint;
        this.ApiKey = ApiKey;
    }

    public required string Endpoint { get; init; }
    public required string ApiKey { get; init; }
}