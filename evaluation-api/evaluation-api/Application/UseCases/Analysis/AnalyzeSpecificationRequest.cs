using evaluation_api.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace evaluation_api.Application.UseCases.Analysis;

/// <summary>
/// Request para analizar una especificación de software con IA
/// </summary>
public class AnalyzeSpecificationRequest
{
    [Required(ErrorMessage = "La especificación es requerida")]
    [MinLength(10, ErrorMessage = "La especificación debe tener al menos 10 caracteres")]
    public string Especificacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de análisis es requerido")]
    public TipoAnalisis TipoAnalisis { get; set; } = TipoAnalisis.Detailed;
}
