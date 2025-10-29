using evaluation_api.Domain.Enums;

namespace evaluation_api.Application.Interfaces;

/// <summary>
/// Interfaz genérica para servicios de IA (OpenRouter, OpenAI, etc.)
/// </summary>
public interface IAIService
{
    Task<AIResponse> GenerateCompletionAsync(AIRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Request genérico para servicios de IA
/// </summary>
public class AIRequest
{
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 4000;
    public string? Model { get; set; }
}

/// <summary>
/// Response genérico de servicios de IA
/// </summary>
public class AIResponse
{
    public string Content { get; set; } = string.Empty;
    public string? Model { get; set; }
    public int? TotalTokens { get; set; }
}
