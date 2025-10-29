using System.Text.Json.Serialization;

namespace evaluation_api.Domain.Enums;

/// <summary>
/// Tipos de análisis que puede realizar la IA
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoAnalisis
{
    Detailed,
    Processes,
    Basic
}
