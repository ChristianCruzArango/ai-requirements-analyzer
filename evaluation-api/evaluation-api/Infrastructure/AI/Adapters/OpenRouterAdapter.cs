using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using evaluation_api.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace evaluation_api.Infrastructure.AI.Adapters;

/// <summary>
/// Adaptador para OpenRouter AI
/// </summary>
public class OpenRouterAdapter : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly string _apiKey;
    private readonly string _defaultModel;

    public OpenRouterAdapter(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiUrl = configuration["OpenRouter:ApiUrl"] ?? "https://openrouter.ai/api/v1";
        _apiKey = configuration["OpenRouter:ApiKey"] ?? "";
        _defaultModel = configuration["OpenRouter:DefaultModel"] ?? "deepseek/deepseek-chat";

        // NO establecer BaseAddress, usar URL completa en cada request
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5000");
        _httpClient.DefaultRequestHeaders.Add("X-Title", "Evaluation IA");
    }

    public async Task<AIResponse> GenerateCompletionAsync(AIRequest request, CancellationToken cancellationToken)
    {
        var openRouterRequest = new OpenRouterRequest
        {
            Model = request.Model ?? _defaultModel,
            Messages = new List<Message>
            {
                new Message { Role = "system", Content = request.SystemPrompt },
                new Message { Role = "user", Content = request.UserPrompt }
            },
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens
        };

        var jsonContent = JsonSerializer.Serialize(openRouterRequest);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Usar URL completa
        var fullUrl = $"{_apiUrl}/chat/completions";
        Console.WriteLine($"🔗 Llamando a OpenRouter: {fullUrl}");
        Console.WriteLine($"🤖 Modelo: {openRouterRequest.Model}");

        HttpResponseMessage response;

        response = await _httpClient.PostAsync(fullUrl, httpContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new evaluation_api.Domain.Exceptions.AIServiceException(
                $"Error en OpenRouter API: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        // Log para debugging
        Console.WriteLine($"OpenRouter Response: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}");

        OpenRouterResponse? openRouterResponse;
        try
        {
            openRouterResponse = JsonSerializer.Deserialize<OpenRouterResponse>(responseContent);
        }
        catch (JsonException ex)
        {
            throw new evaluation_api.Domain.Exceptions.AIServiceException(
                $"Error al deserializar respuesta de OpenRouter. Respuesta recibida: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}. Error: {ex.Message}");
        }

        if (openRouterResponse?.Choices == null || openRouterResponse.Choices.Count == 0)
        {
            throw new evaluation_api.Domain.Exceptions.AIServiceException(
                $"No se recibió respuesta válida de OpenRouter AI. Respuesta: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}");
        }

        return new AIResponse
        {
            Content = openRouterResponse.Choices[0].Message.Content,
            Model = openRouterResponse.Model,
            TotalTokens = openRouterResponse.Usage?.TotalTokens
        };
    }
}

// DTOs específicos de OpenRouter
internal class OpenRouterRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = new();

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }
}

internal class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

internal class OpenRouterResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }
}

internal class Choice
{
    [JsonPropertyName("message")]
    public Message Message { get; set; } = new();

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

internal class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
