using CoralLedger.Application.Common.Interfaces;

namespace CoralLedger.Web.Endpoints;

public static class AIEndpoints
{
    public static IEndpointRouteBuilder MapAIEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/ai")
            .WithTags("AI Assistant");

        // GET /api/ai/status - Check if AI is configured
        group.MapGet("/status", (IMarineAIService aiService) =>
        {
            return Results.Ok(new
            {
                configured = aiService.IsConfigured,
                message = aiService.IsConfigured
                    ? "AI assistant is ready"
                    : "AI assistant is not configured. Set MarineAI:ApiKey in configuration."
            });
        })
        .WithName("GetAIStatus")
        .WithDescription("Check if AI service is configured and available")
        .Produces<object>();

        // POST /api/ai/query - Submit natural language query
        group.MapPost("/query", async (
            AIQueryRequest request,
            IMarineAIService aiService,
            CancellationToken ct = default) =>
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return Results.BadRequest(new { error = "Query is required" });
            }

            if (request.Query.Length > 500)
            {
                return Results.BadRequest(new { error = "Query must be 500 characters or less" });
            }

            var result = await aiService.QueryAsync(request.Query, ct);

            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(new
            {
                query = request.Query,
                answer = result.Answer,
                data = result.Data
            });
        })
        .WithName("QueryAI")
        .WithDescription("Submit a natural language query about marine data")
        .Produces<object>()
        .Produces(StatusCodes.Status400BadRequest);

        // GET /api/ai/suggestions - Get suggested queries
        group.MapGet("/suggestions", async (
            IMarineAIService aiService,
            CancellationToken ct = default) =>
        {
            var suggestions = await aiService.GetSuggestedQueriesAsync(ct);
            return Results.Ok(new { suggestions });
        })
        .WithName("GetAISuggestions")
        .WithDescription("Get suggested natural language queries")
        .Produces<object>();

        return endpoints;
    }
}

public record AIQueryRequest(string Query);
