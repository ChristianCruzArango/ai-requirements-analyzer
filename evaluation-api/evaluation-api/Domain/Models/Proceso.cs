namespace evaluation_api.Domain.Models;

/// <summary>
/// Representa un proceso principal del sistema analizado
/// </summary>
public class Proceso
{
    public int? IdProceso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public List<Subproceso> Subprocesos { get; set; } = new();

    public Proceso()
    {
    }

    public Proceso(string nombre, string? descripcion = null)
    {
        Nombre = nombre;
        Descripcion = descripcion;
    }
}
