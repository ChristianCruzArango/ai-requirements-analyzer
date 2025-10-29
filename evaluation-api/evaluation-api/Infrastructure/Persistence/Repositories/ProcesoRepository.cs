using Microsoft.EntityFrameworkCore;
using evaluation_api.Application.Interfaces;
using evaluation_api.Domain.Models;
using evaluation_api.Infrastructure.Persistence.Mappers;

namespace evaluation_api.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de procesos usando Entity Framework Core y PostgreSQL
/// </summary>
public class ProcesoRepository : IProcesoRepository
{
    private readonly AppDbContext _context;

    public ProcesoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateAsync(Proceso proceso, CancellationToken cancellationToken)
    {
        var entity = ProcesoMapper.ToEntity(proceso);

        await _context.Procesos.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.IdProceso;
    }

    public async Task<Proceso?> GetByIdAsync(int idProceso, CancellationToken cancellationToken)
    {
        var entity = await _context.Procesos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProceso == idProceso, cancellationToken);

        return entity is null ? null : ProcesoMapper.ToDomain(entity);
    }

    public async Task<List<Proceso>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _context.Procesos
            .AsNoTracking()
            .OrderByDescending(p => p.IdProceso)
            .ToListAsync(cancellationToken);

        return entities.Select(ProcesoMapper.ToDomain).ToList();
    }

    public async Task DeleteAsync(int idProceso, CancellationToken cancellationToken)
    {
        var entity = await _context.Procesos.FindAsync(new object[] { idProceso }, cancellationToken);
        if (entity != null)
        {
            _context.Procesos.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
