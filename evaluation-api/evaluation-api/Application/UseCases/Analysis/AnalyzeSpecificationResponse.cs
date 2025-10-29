using evaluation_api.Domain.Models;

namespace evaluation_api.Application.UseCases.Analysis;

/// <summary>
/// Response del análisis de especificación
/// </summary>
public record AnalyzeSpecificationResponse(
    List<Proceso> Procesos,
    string? Resumen,
    List<string>? Recomendaciones
);
