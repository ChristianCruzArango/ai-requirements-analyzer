using evaluation_api.Domain.Models;

namespace evaluation_api.Application.Interfaces;

/// <summary>
/// Contrato de acceso a procesos para la capa de Aplicación
/// </summary>
public interface IProcesoRepository
{
    Task<int> CreateAsync(Proceso proceso, CancellationToken cancellationToken);
    Task<Proceso?> GetByIdAsync(int idProceso, CancellationToken cancellationToken);
    Task<List<Proceso>> GetAllAsync(CancellationToken cancellationToken);
    Task DeleteAsync(int idProceso, CancellationToken cancellationToken);
}
