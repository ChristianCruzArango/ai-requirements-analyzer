using Microsoft.EntityFrameworkCore;
using evaluation_api.Application.Interfaces;
using evaluation_api.Domain.Models;
using evaluation_api.Infrastructure.Persistence.Mappers;

namespace evaluation_api.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de casos de uso usando Entity Framework Core y PostgreSQL
/// </summary>
public class CasoUsoRepository : ICasoUsoRepository
{
    private readonly AppDbContext _context;

    public CasoUsoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateAsync(CasoUso casoUso, CancellationToken cancellationToken)
    {
        var entity = CasoUsoMapper.ToEntity(casoUso);

        await _context.CasosUso.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.IdCasoUso;
    }

    public async Task<CasoUso?> GetByIdAsync(int idCasoUso, CancellationToken cancellationToken)
    {
        var entity = await _context.CasosUso
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCasoUso == idCasoUso, cancellationToken);

        return entity is null ? null : CasoUsoMapper.ToDomain(entity);
    }

    public async Task<List<CasoUso>> GetBySubprocesoIdAsync(int idSubproceso, CancellationToken cancellationToken)
    {
        var entities = await _context.CasosUso
            .AsNoTracking()
            .Where(c => c.IdSubproceso == idSubproceso)
            .OrderBy(c => c.IdCasoUso)
            .ToListAsync(cancellationToken);

        return entities.Select(CasoUsoMapper.ToDomain).ToList();
    }

    public async Task DeleteBySubprocesoIdAsync(int idSubproceso, CancellationToken cancellationToken)
    {
        var entities = await _context.CasosUso
            .Where(c => c.IdSubproceso == idSubproceso)
            .ToListAsync(cancellationToken);

        _context.CasosUso.RemoveRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
