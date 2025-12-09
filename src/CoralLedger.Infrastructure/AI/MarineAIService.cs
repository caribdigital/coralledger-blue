using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CoralLedger.Application.Common.Interfaces;
using CoralLedger.Domain.Entities;
using CoralLedger.Domain.Enums;
using CoralLedger.Infrastructure.AI.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace CoralLedger.Infrastructure.AI;

/// <summary>
/// AI service for natural language queries about marine data
/// Sprint 5.1: Enhanced with query interpretation, disambiguation, and security (US-5.1.2, US-5.1.4, US-5.1.5)
/// </summary>
public class MarineAIService : IMarineAIService
{
    private readonly MarineAIOptions _options;
    private readonly IMarineDbContext _context;
    private readonly ILogger<MarineAIService> _logger;
    private readonly Kernel? _kernel;

    // Cache for interpreted queries (in production, use Redis)
    private static readonly ConcurrentDictionary<string, CachedInterpretation> _interpretationCache = new();

    private const string BaseSystemPrompt = @"
You are a marine conservation AI assistant for CoralLedger Blue, a platform focused on protecting
the marine ecosystems of the Bahamas. You help users query data about:

- Marine Protected Areas (MPAs) - boundaries, protection levels, island groups
- Coral bleaching alerts - NOAA data on sea surface temperature and bleaching risk
- Fishing activity - vessel events detected via Global Fishing Watch
- Reef health - status of monitored coral reefs
- Citizen observations - reports from community scientists
- Bahamian species database - invasive species (Lionfish), threatened/endangered species

When answering questions:
1. Use the available functions to query actual data from the database
2. Provide specific numbers and names when available
3. Explain the significance of findings for marine conservation
4. If asked about locations, use spatial functions to check MPA boundaries
5. Be concise but informative

The Bahamas has approximately 30 Marine Protected Areas covering various island groups including
the Exumas, Andros, Abaco, and areas around Nassau.
";

    private const string InterpretationPrompt = @"
Analyze this natural language query and describe what data will be retrieved.
Return a JSON object with this structure:
{
    ""interpretation"": ""Brief description of what you understand the query to mean"",
    ""dataSources"": [""list"", ""of"", ""data sources""],
    ""needsDisambiguation"": false,
    ""disambiguationNeeded"": null
}

If the query contains vague terms like 'healthy', 'recent', 'nearby', 'high risk', set needsDisambiguation to true
and provide disambiguationNeeded as an object with:
{
    ""term"": ""the vague term"",
    ""question"": ""clarifying question"",
    ""options"": [""option1"", ""option2"", ""option3""]
}

Examples of vague terms needing disambiguation:
- 'healthy' -> Low bleaching? High coral cover? Active fish population?
- 'recent' -> Last 24 hours? Past week? Past month?
- 'nearby' -> Within 5km? Within 20km? Same island group?
- 'high risk' -> DHW > 4? Alert Level 2+? Bleaching warning?

Only return the JSON, no other text.
";

    // Vague terms that trigger disambiguation (US-5.1.4)
    private static readonly Dictionary<string, DisambiguationOption> VagueTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        ["healthy"] = new DisambiguationOption(
            "healthy",
            "By 'healthy', do you mean:",
            new[] { "Low bleaching risk (DHW < 2)", "High coral cover (>30%)", "Active fish population" }),
        ["recent"] = new DisambiguationOption(
            "recent",
            "By 'recent', what time period do you mean:",
            new[] { "Last 24 hours", "Past 7 days", "Past 30 days" }),
        ["nearby"] = new DisambiguationOption(
            "nearby",
            "By 'nearby', what distance do you mean:",
            new[] { "Within 5 km", "Within 20 km", "Same island group" }),
        ["high risk"] = new DisambiguationOption(
            "high risk",
            "By 'high risk', do you mean:",
            new[] { "DHW > 4 (significant bleaching)", "Alert Level 2+ (warning)", "Any bleaching watch or higher" }),
        ["stressed"] = new DisambiguationOption(
            "stressed",
            "By 'stressed', do you mean:",
            new[] { "Bleaching watch active", "SST anomaly > 1°C", "DHW between 2-4" }),
        ["active"] = new DisambiguationOption(
            "active",
            "By 'active', what time period do you mean:",
            new[] { "Currently happening", "Past 24 hours", "Past week" })
    };

    // Security-restricted terms (US-5.1.5)
    private static readonly string[] SensitiveTerms =
    {
        "enforcement",
        "patrol route",
        "confidential",
        "poacher",
        "illegal vessel",
        "investigation",
        "arrest",
        "citation"
    };

    private static readonly Dictionary<UserPersona, string> PersonaPrompts = new()
    {
        [UserPersona.General] = "",
        [UserPersona.Ranger] = @"

USER PERSONA: PARK RANGER
You are speaking to a field enforcement officer. Adapt your responses to:
- Prioritize enforcement and legal compliance information
- Highlight unauthorized activities, violations, and boundary breaches
- Provide clear coordinates and locations for patrol planning
- Include actionable field intelligence with specific vessel identifications
- Flag any NoTake zone violations or suspicious activities
- Format response with ACTION ITEMS when applicable
- Keep language direct and operational",

        [UserPersona.Fisherman] = @"

USER PERSONA: FISHERMAN
You are speaking to a commercial fisherman. Adapt your responses to:
- Focus on fishing activity, sustainable catch areas, and gear regulations
- Use plain language - avoid technical jargon
- Explain how conditions affect fishing (e.g., 'waters are too warm for fish to feed' not 'SST anomaly +2.1°C')
- Highlight protected zones where fishing is restricted
- Provide practical information about seasons and quotas
- Be respectful of traditional fishing knowledge
- Include Bahamian local names for species when available",

        [UserPersona.Scientist] = @"

USER PERSONA: SCIENTIST/RESEARCHER
You are speaking to a marine researcher. Adapt your responses to:
- Include data sources and methodology notes (NOAA, Global Fishing Watch, etc.)
- Provide statistical context: sample sizes, confidence levels, temporal ranges
- Use precise scientific terminology and species names (scientific names)
- Note data limitations and uncertainty ranges
- Reference IUCN conservation status classifications
- Include DHW (Degree Heating Weeks), SST values, and other quantitative metrics
- Mention spatial analysis methods used (PostGIS functions, coordinate systems)",

        [UserPersona.Policymaker] = @"

USER PERSONA: POLICYMAKER
You are speaking to a government official or policy advisor. Adapt your responses to:
- Lead with executive summary of key findings
- Frame information in terms of policy implications and outcomes
- Highlight trends and strategic patterns
- Provide quantitative impact metrics and comparisons
- Include recommendations for regulatory or conservation actions
- Focus on ecosystem health and economic implications
- Connect findings to Bahamas marine protection goals and international commitments"
    };

    private string GetSystemPrompt(UserPersona persona) =>
        BaseSystemPrompt + PersonaPrompts.GetValueOrDefault(persona, "");

    public MarineAIService(
        IOptions<MarineAIOptions> options,
        IMarineDbContext context,
        ILogger<MarineAIService> logger)
    {
        _options = options.Value;
        _context = context;
        _logger = logger;

        if (_options.Enabled && !string.IsNullOrEmpty(_options.ApiKey))
        {
            try
            {
                var builder = Kernel.CreateBuilder();

                if (_options.UseAzureOpenAI && !string.IsNullOrEmpty(_options.AzureEndpoint))
                {
                    builder.AddAzureOpenAIChatCompletion(
                        deploymentName: _options.ModelId,
                        endpoint: _options.AzureEndpoint,
                        apiKey: _options.ApiKey);
                }
                else
                {
                    builder.AddOpenAIChatCompletion(
                        modelId: _options.ModelId,
                        apiKey: _options.ApiKey);
                }

                _kernel = builder.Build();

                // Register plugins
                _kernel.Plugins.AddFromObject(new MarineDataPlugin(_context), "MarineData");
                _kernel.Plugins.AddFromObject(new SpatialQueryPlugin(_context), "SpatialQuery");

                _logger.LogInformation("MarineAI service initialized with model {Model}", _options.ModelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize MarineAI service");
            }
        }
    }

    public bool IsConfigured => _kernel != null;

    /// <summary>
    /// Interpret a query before execution (US-5.1.2)
    /// "I understood this as: Reefs with DHW > 4 in Exumas. Correct?"
    /// </summary>
    public async Task<QueryInterpretation> InterpretQueryAsync(
        string naturalLanguageQuery,
        UserPersona persona = UserPersona.General,
        CancellationToken cancellationToken = default)
    {
        var interpretationId = Guid.NewGuid().ToString("N")[..12];

        // Create audit log entry
        var auditLog = NLQAuditLog.Create(naturalLanguageQuery, persona);

        // Check for security-restricted terms (US-5.1.5)
        var securityWarning = CheckSecurityRestrictions(naturalLanguageQuery, persona);
        if (securityWarning != null)
        {
            auditLog.MarkSecurityBlocked(securityWarning);
            await SaveAuditLog(auditLog, cancellationToken);

            return new QueryInterpretation(
                interpretationId,
                naturalLanguageQuery,
                "Query blocked due to security restrictions",
                Array.Empty<string>(),
                null,
                false,
                securityWarning,
                persona);
        }

        // Check for vague terms (US-5.1.4)
        var disambiguations = DetectVagueTerms(naturalLanguageQuery);

        // Determine data sources that will be used
        var dataSources = DetermineDataSources(naturalLanguageQuery);

        // Generate interpretation using AI if configured
        string interpretation;
        if (IsConfigured)
        {
            interpretation = await GenerateInterpretationAsync(naturalLanguageQuery, cancellationToken);
        }
        else
        {
            interpretation = GenerateBasicInterpretation(naturalLanguageQuery, dataSources);
        }

        auditLog.MarkInterpreted(interpretation, dataSources, disambiguations.Count > 0);
        await SaveAuditLog(auditLog, cancellationToken);

        // Cache the interpretation for later execution
        _interpretationCache[interpretationId] = new CachedInterpretation(
            naturalLanguageQuery,
            persona,
            DateTime.UtcNow.AddMinutes(10),
            auditLog.Id);

        return new QueryInterpretation(
            interpretationId,
            naturalLanguageQuery,
            interpretation,
            dataSources,
            disambiguations.Count > 0 ? disambiguations : null,
            disambiguations.Count > 0,
            null,
            persona);
    }

    /// <summary>
    /// Execute a previously interpreted query
    /// </summary>
    public async Task<MarineQueryResult> ExecuteInterpretedQueryAsync(
        string interpretationId,
        CancellationToken cancellationToken = default)
    {
        if (!_interpretationCache.TryGetValue(interpretationId, out var cached))
        {
            return new MarineQueryResult(false, Error: "Interpretation not found or expired. Please interpret the query again.");
        }

        if (cached.ExpiresAt < DateTime.UtcNow)
        {
            _interpretationCache.TryRemove(interpretationId, out _);
            return new MarineQueryResult(false, Error: "Interpretation expired. Please interpret the query again.");
        }

        // Execute the actual query
        var result = await QueryAsync(cached.Query, cached.Persona, cancellationToken);

        // Clean up cache
        _interpretationCache.TryRemove(interpretationId, out _);

        return result;
    }

    public Task<MarineQueryResult> QueryAsync(
        string naturalLanguageQuery,
        CancellationToken cancellationToken = default) =>
        QueryAsync(naturalLanguageQuery, UserPersona.General, cancellationToken);

    public async Task<MarineQueryResult> QueryAsync(
        string naturalLanguageQuery,
        UserPersona persona,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // Create audit log entry
        var auditLog = NLQAuditLog.Create(naturalLanguageQuery, persona);

        // Check for security-restricted terms (US-5.1.5)
        var securityWarning = CheckSecurityRestrictions(naturalLanguageQuery, persona);
        if (securityWarning != null)
        {
            auditLog.MarkSecurityBlocked(securityWarning);
            await SaveAuditLog(auditLog, cancellationToken);
            return new MarineQueryResult(false, Error: securityWarning);
        }

        if (!IsConfigured)
        {
            auditLog.MarkFailed("AI service not configured", (int)stopwatch.ElapsedMilliseconds);
            await SaveAuditLog(auditLog, cancellationToken);
            return new MarineQueryResult(false, Error: "AI service is not configured. Please set MarineAI:ApiKey in configuration.");
        }

        try
        {
            var chatService = _kernel!.GetRequiredService<IChatCompletionService>();

            var systemPrompt = GetSystemPrompt(persona);
            var chatHistory = new ChatHistory(systemPrompt);
            chatHistory.AddUserMessage(naturalLanguageQuery);

            var settings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = _options.MaxTokens,
                Temperature = _options.Temperature,
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                settings,
                _kernel,
                cancellationToken);

            stopwatch.Stop();

            // Determine what data sources were used
            var dataSources = DetermineDataSources(naturalLanguageQuery);
            var interpretation = GenerateBasicInterpretation(naturalLanguageQuery, dataSources);

            auditLog.MarkInterpreted(interpretation, dataSources);
            auditLog.MarkExecuted(null, (int)stopwatch.ElapsedMilliseconds);
            await SaveAuditLog(auditLog, cancellationToken);

            _logger.LogInformation(
                "AI query processed for persona {Persona} in {Ms}ms: {Query}",
                persona,
                stopwatch.ElapsedMilliseconds,
                naturalLanguageQuery);

            return new MarineQueryResult(
                Success: true,
                Answer: response.Content,
                Persona: persona,
                InterpretedAs: interpretation);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            auditLog.MarkFailed(ex.Message, (int)stopwatch.ElapsedMilliseconds);
            await SaveAuditLog(auditLog, cancellationToken);

            _logger.LogError(ex, "Error processing AI query for persona {Persona}: {Query}", persona, naturalLanguageQuery);
            return new MarineQueryResult(false, Error: ex.Message);
        }
    }

    public Task<IReadOnlyList<string>> GetSuggestedQueriesAsync(
        CancellationToken cancellationToken = default)
    {
        var suggestions = new List<string>
        {
            "How many Marine Protected Areas are in the Bahamas?",
            "Which MPAs have NoTake protection level?",
            "Show me the latest bleaching alerts",
            "What fishing activity has been detected in the past week?",
            "Find MPAs near Nassau (longitude -77.35, latitude 25.05)",
            "Is the location -77.5, 24.25 inside any MPA?",
            "What is the total protected marine area?",
            "Show citizen observations from the past month",
            "Which areas have high bleaching risk?",
            "List MPAs in the Exumas island group"
        };

        return Task.FromResult<IReadOnlyList<string>>(suggestions);
    }

    /// <summary>
    /// Check for security-restricted queries (US-5.1.5)
    /// </summary>
    private string? CheckSecurityRestrictions(string query, UserPersona persona)
    {
        var queryLower = query.ToLowerInvariant();

        foreach (var term in SensitiveTerms)
        {
            if (queryLower.Contains(term))
            {
                // Only Rangers can query enforcement data
                if (persona != UserPersona.Ranger)
                {
                    _logger.LogWarning(
                        "Security restriction: Non-ranger persona {Persona} attempted to query sensitive term '{Term}'",
                        persona,
                        term);
                    return $"Access to enforcement-related data is restricted. This query contains sensitive terms ('{term}') that require Ranger-level access.";
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Detect vague terms that need disambiguation (US-5.1.4)
    /// </summary>
    private List<DisambiguationOption> DetectVagueTerms(string query)
    {
        var found = new List<DisambiguationOption>();
        var queryLower = query.ToLowerInvariant();

        foreach (var (term, disambiguation) in VagueTerms)
        {
            if (Regex.IsMatch(queryLower, $@"\b{Regex.Escape(term)}\b"))
            {
                found.Add(disambiguation);
            }
        }

        return found;
    }

    /// <summary>
    /// Determine which data sources will be used based on query keywords
    /// </summary>
    private List<string> DetermineDataSources(string query)
    {
        var sources = new List<string>();
        var queryLower = query.ToLowerInvariant();

        if (queryLower.Contains("mpa") || queryLower.Contains("protected area") || queryLower.Contains("boundary"))
            sources.Add("Marine Protected Areas database");

        if (queryLower.Contains("bleach") || queryLower.Contains("coral") || queryLower.Contains("dhw") || queryLower.Contains("temperature"))
            sources.Add("NOAA Coral Reef Watch (bleaching alerts)");

        if (queryLower.Contains("fish") || queryLower.Contains("vessel") || queryLower.Contains("boat") || queryLower.Contains("activity"))
            sources.Add("Global Fishing Watch (vessel tracking)");

        if (queryLower.Contains("reef") || queryLower.Contains("health"))
            sources.Add("Reef health monitoring data");

        if (queryLower.Contains("observation") || queryLower.Contains("citizen") || queryLower.Contains("report"))
            sources.Add("Citizen science observations");

        if (queryLower.Contains("species") || queryLower.Contains("lionfish") || queryLower.Contains("invasive") || queryLower.Contains("endangered"))
            sources.Add("Bahamian species database");

        if (queryLower.Contains("location") || queryLower.Contains("near") || queryLower.Contains("distance") || queryLower.Contains("within"))
            sources.Add("PostGIS spatial analysis");

        if (sources.Count == 0)
            sources.Add("General marine database");

        return sources;
    }

    /// <summary>
    /// Generate basic interpretation without AI
    /// </summary>
    private string GenerateBasicInterpretation(string query, IEnumerable<string> dataSources)
    {
        var sources = string.Join(", ", dataSources);
        return $"Query: \"{query}\" - Will search: {sources}";
    }

    /// <summary>
    /// Generate AI-powered interpretation
    /// </summary>
    private async Task<string> GenerateInterpretationAsync(string query, CancellationToken cancellationToken)
    {
        try
        {
            var chatService = _kernel!.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory(InterpretationPrompt);
            chatHistory.AddUserMessage(query);

            var settings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = 300,
                Temperature = 0.1
            };

            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                settings,
                cancellationToken: cancellationToken);

            return response.Content ?? GenerateBasicInterpretation(query, DetermineDataSources(query));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate AI interpretation, using basic");
            return GenerateBasicInterpretation(query, DetermineDataSources(query));
        }
    }

    private async Task SaveAuditLog(NLQAuditLog auditLog, CancellationToken cancellationToken)
    {
        try
        {
            _context.NLQAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Log but don't fail the main operation
            _logger.LogWarning(ex, "Failed to save NLQ audit log");
        }
    }

    private record CachedInterpretation(
        string Query,
        UserPersona Persona,
        DateTime ExpiresAt,
        Guid AuditLogId);
}
