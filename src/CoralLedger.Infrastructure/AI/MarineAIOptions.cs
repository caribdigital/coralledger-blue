namespace CoralLedger.Infrastructure.AI;

public class MarineAIOptions
{
    public const string SectionName = "MarineAI";

    /// <summary>
    /// Enable AI features (requires valid API key)
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// OpenAI API key or Azure OpenAI API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model deployment name (e.g., "gpt-4o-mini", "gpt-4o")
    /// </summary>
    public string ModelId { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// Use Azure OpenAI instead of OpenAI
    /// </summary>
    public bool UseAzureOpenAI { get; set; } = false;

    /// <summary>
    /// Azure OpenAI endpoint (required if UseAzureOpenAI is true)
    /// </summary>
    public string? AzureEndpoint { get; set; }

    /// <summary>
    /// Maximum tokens for responses
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Temperature for response creativity (0.0 = deterministic, 1.0 = creative)
    /// </summary>
    public double Temperature { get; set; } = 0.3;
}
