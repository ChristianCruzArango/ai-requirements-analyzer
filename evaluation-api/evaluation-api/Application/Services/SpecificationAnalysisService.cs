using System.Text.Json;
using evaluation_api.Application.Interfaces;
using evaluation_api.Domain.Enums;
using evaluation_api.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace evaluation_api.Application.Services;

/// <summary>
/// Servicio de análisis de especificaciones que usa IAIService (adaptador)
/// </summary>
public class SpecificationAnalysisService : ISpecificationAnalysisService
{
    private readonly IAIService _aiService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SpecificationAnalysisService> _logger;

    public SpecificationAnalysisService(
        IAIService aiService,
        IConfiguration configuration,
        ILogger<SpecificationAnalysisService> logger)
    {
        _aiService = aiService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AnalysisResult> AnalyzeSpecificationAsync(
        string specification,
        TipoAnalisis tipoAnalisis,
        CancellationToken cancellationToken)
    {
        var systemPrompt = BuildSystemPrompt(tipoAnalisis);
        var userPrompt = BuildUserPrompt(specification, tipoAnalisis);

        var aiRequest = new AIRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            Temperature = 0.7,
            MaxTokens = 4000
        };

        _logger.LogInformation("🤖 Enviando solicitud a IA...");

        var aiResponse = await _aiService.GenerateCompletionAsync(aiRequest, cancellationToken);

        _logger.LogInformation("✅ Respuesta recibida de IA. Tokens: {Tokens}", aiResponse.TotalTokens);

        return ParseAIResponse(aiResponse.Content, tipoAnalisis);
    }

    private string BuildSystemPrompt(TipoAnalisis tipoAnalisis)
    {
        var basePrompt = _configuration["AIPrompts:BasePrompt"] ?? string.Empty;

        var (instructions, jsonStructure, notes) = tipoAnalisis switch
        {
            TipoAnalisis.Detailed => (
                _configuration["AIPrompts:DetailedAnalysis:Instructions"],
                _configuration["AIPrompts:DetailedAnalysis:JsonStructure"],
                _configuration["AIPrompts:DetailedAnalysis:Notes"]
            ),
            TipoAnalisis.Processes => (
                _configuration["AIPrompts:ProcessesAnalysis:Instructions"],
                _configuration["AIPrompts:ProcessesAnalysis:JsonStructure"],
                null
            ),
            TipoAnalisis.Basic => (
                _configuration["AIPrompts:BasicAnalysis:Instructions"],
                _configuration["AIPrompts:BasicAnalysis:JsonStructure"],
                null
            ),
            _ => (null, null, null)
        };

        var prompt = $"{basePrompt}\n\n{instructions}\n\nEl JSON debe tener esta estructura exacta:\n{jsonStructure}";

        if (!string.IsNullOrEmpty(notes))
        {
            prompt += $"\n\n{notes}";
        }

        return prompt;
    }

    private string BuildUserPrompt(string specification, TipoAnalisis tipoAnalisis)
    {
        var analysisType = tipoAnalisis switch
        {
            TipoAnalisis.Detailed => "análisis detallado con procesos, subprocesos y casos de uso",
            TipoAnalisis.Processes => "análisis de procesos y subprocesos",
            TipoAnalisis.Basic => "análisis básico de procesos",
            _ => "análisis"
        };

        var template = _configuration["AIPrompts:UserPromptTemplate"] ?? string.Empty;
        return string.Format(template, analysisType, specification);
    }

    private AnalysisResult ParseAIResponse(string aiResponse, TipoAnalisis tipoAnalisis)
    {
        // Limpiar la respuesta de posibles markdown code blocks
        var cleanedResponse = aiResponse.Trim();
        if (cleanedResponse.StartsWith("```json"))
        {
            cleanedResponse = cleanedResponse.Replace("```json", "").Replace("```", "").Trim();
        }
        else if (cleanedResponse.StartsWith("```"))
        {
            cleanedResponse = cleanedResponse.Replace("```", "").Trim();
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        AnalysisResult? result;

        result = JsonSerializer.Deserialize<AnalysisResult>(cleanedResponse, options);

        if (result == null || result.Procesos == null)
        {
            _logger.LogError("❌ Failed to parse AI response. Response: {Response}", cleanedResponse);
            throw new Domain.Exceptions.AIServiceException("No se pudo parsear la respuesta de la IA como JSON válido");
        }

        return result;
    }
}
