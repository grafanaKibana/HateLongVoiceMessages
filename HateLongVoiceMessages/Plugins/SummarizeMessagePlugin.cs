#pragma warning disable SKEXP0050
namespace HateLongVoiceMessages.Plugins;

using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Text;

public class SummarizeMessagePlugin
{
    public static string Language => "RU";

    private const string Prompt = """
                                  LANGUAGE: {{$LANGUAGE}}

                                  BEGIN CONTENT TO SUMMARIZE:
                                  {{$INPUT}}

                                  END CONTENT TO SUMMARIZE.

                                  Summarize the message transcript in 'CONTENT TO SUMMARIZE', into a concise, small-to-medium-length summary that includes all key points and conclusions without missing any important details.
                                  Summary should be in the language that defined in the 'LANGUAGE',using plain text and complete sentences without any markup or tags.
                                  Present the summary from the first-person perspective, as if the original speaker is summarizing their own message. 
                                  Do not treat the content as a conversation transcript, and avoid incorporating any general knowledge beyond what is provided."

                                  BEGIN SUMMARY:
                                  """;

        /// <summary>
    /// The max tokens to process in a single prompt function call.
    /// </summary>
    private const int MaxTokens = 1024;

    private KernelFunction SummarizeMessageTranscriptFunction { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversationSummaryPlugin"/> class.
    /// </summary>
    public SummarizeMessagePlugin()
    {
        PromptExecutionSettings settings = new()
        {
            ExtensionData = new Dictionary<string, object>
            {
                { "Temperature", 0.1 },
                { "TopP", 0.5 },
                { "MaxTokens", MaxTokens }
            }
        };

        this.SummarizeMessageTranscriptFunction = KernelFunctionFactory.CreateFromPrompt(
            Prompt,
            description: "Given a section of a voice message transcript, summarize it.",
            executionSettings: settings);
    }

    /// <summary>
    /// Given a long conversation transcript, summarize the conversation.
    /// </summary>
    /// <param name="input">A long conversation transcript.</param>
    /// <param name="kernel">The <see cref="Kernel"/> containing services, plugins, and other state for use throughout the operation.</param>
    [KernelFunction, Description("Given a voice message transcript, summarize the conversation.")]
    public Task<string> SummarizeMessageTranscriptAsync([Description("A voice message transcript.")] string input, Kernel kernel) =>
        ProcessAsync(this.SummarizeMessageTranscriptFunction, input, kernel);

    private static async Task<string> ProcessAsync(KernelFunction func, string input, Kernel kernel)
    {
        var lines = TextChunker.SplitPlainTextLines(input, MaxTokens);
        var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, MaxTokens);

        var results = new string[paragraphs.Count];

        for (var i = 0; i < results.Length; i++)
        {
            // The first parameter is the input text.
            results[i] = (await func.InvokeAsync(kernel, new KernelArguments { ["input"] = paragraphs[i] }).ConfigureAwait(false))
                .GetValue<string>() ?? string.Empty;
        }

        return string.Join("\n", results);
    }
}