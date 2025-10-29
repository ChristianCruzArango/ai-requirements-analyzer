using evaluation_api.Domain.Models;

namespace evaluation_api.Application.Interfaces;

/// <summary>
/// Contrato de acceso a casos de uso para la capa de Aplicación
/// </summary>
public interface ICasoUsoRepository
{
    Task<int> CreateAsync(CasoUso casoUso, CancellationToken cancellationToken);
    Task<CasoUso?> GetByIdAsync(int idCasoUso, CancellationToken cancellationToken);
    Task<List<CasoUso>> GetBySubprocesoIdAsync(int idSubproceso, CancellationToken cancellationToken);
    Task DeleteBySubprocesoIdAsync(int idSubproceso, CancellationToken cancellationToken);
}
