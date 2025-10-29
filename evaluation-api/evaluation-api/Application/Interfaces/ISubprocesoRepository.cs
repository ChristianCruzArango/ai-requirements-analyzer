using evaluation_api.Domain.Models;

namespace evaluation_api.Application.Interfaces;

/// <summary>
/// Contrato de acceso a subprocesos para la capa de Aplicación
/// </summary>
public interface ISubprocesoRepository
{
    Task<int> CreateAsync(Subproceso subproceso, CancellationToken cancellationToken);
    Task<Subproceso?> GetByIdAsync(int idSubproceso, CancellationToken cancellationToken);
    Task<List<Subproceso>> GetByProcesoIdAsync(int idProceso, CancellationToken cancellationToken);
    Task DeleteByProcesoIdAsync(int idProceso, CancellationToken cancellationToken);
}
