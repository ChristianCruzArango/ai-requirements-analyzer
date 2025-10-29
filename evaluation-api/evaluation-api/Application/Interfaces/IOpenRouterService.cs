using evaluation_api.Domain.Models;
using evaluation_api.Domain.Enums;

namespace evaluation_api.Application.Interfaces;

/// <summary>
/// Servicio de análisis de especificaciones usando IA
/// </summary>
public interface ISpecificationAnalysisService
{
    Task<AnalysisResult> AnalyzeSpecificationAsync(string specification, TipoAnalisis tipoAnalisis, CancellationToken cancellationToken);
}

/// <summary>
/// Resultado del análisis de IA
/// </summary>
public class AnalysisResult
{
    public List<Proceso> Procesos { get; set; } = new();
    public string? Resumen { get; set; }
    public List<string>? Recomendaciones { get; set; }
}
