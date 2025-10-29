using Microsoft.EntityFrameworkCore;
using evaluation_api.Application.Interfaces;
using evaluation_api.Domain.Models;
using evaluation_api.Infrastructure.Persistence.Mappers;

namespace evaluation_api.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de subprocesos usando Entity Framework Core y PostgreSQL
/// </summary>
public class SubprocesoRepository : ISubprocesoRepository
{
    private readonly AppDbContext _context;

    public SubprocesoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateAsync(Subproceso subproceso, CancellationToken cancellationToken)
    {
        var entity = SubprocesoMapper.ToEntity(subproceso);

        await _context.Subprocesos.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.IdSubproceso;
    }

    public async Task<Subproceso?> GetByIdAsync(int idSubproceso, CancellationToken cancellationToken)
    {
        var entity = await _context.Subprocesos
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdSubproceso == idSubproceso, cancellationToken);

        return entity is null ? null : SubprocesoMapper.ToDomain(entity);
    }

    public async Task<List<Subproceso>> GetByProcesoIdAsync(int idProceso, CancellationToken cancellationToken)
    {
        var entities = await _context.Subprocesos
            .AsNoTracking()
            .Where(s => s.IdProceso == idProceso)
            .OrderBy(s => s.IdSubproceso)
            .ToListAsync(cancellationToken);

        return entities.Select(SubprocesoMapper.ToDomain).ToList();
    }

    public async Task DeleteByProcesoIdAsync(int idProceso, CancellationToken cancellationToken)
    {
        var entities = await _context.Subprocesos
            .Where(s => s.IdProceso == idProceso)
            .ToListAsync(cancellationToken);

        _context.Subprocesos.RemoveRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
