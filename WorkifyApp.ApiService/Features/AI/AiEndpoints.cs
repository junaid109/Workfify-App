using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using WorkifyApp.ApiService.Data;

namespace WorkifyApp.ApiService.Features.AI;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai")
            .WithTags("AI")
            .WithOpenApi();

        group.MapPost("/chat", ChatAsync);
    }

    static async Task<Ok<string>> ChatAsync([FromBody] ChatRequest request, WorkifyDbContext db, Kernel kernel)
    {
        // Import the plugin with the current scoped DbContext
        kernel.ImportPluginFromObject(new WorkifyDataPlugin(db), "WorkifyData");

        // Enable auto-invocation of functions
        OpenAIPromptExecutionSettings settings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddUserMessage(request.Message);

        var result = await chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: settings,
            kernel: kernel);

        return TypedResults.Ok(result.Content ?? "");
    }
}

public record ChatRequest(string Message);
