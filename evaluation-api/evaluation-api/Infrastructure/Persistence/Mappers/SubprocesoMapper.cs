using evaluation_api.Domain.Models;
using evaluation_api.Infrastructure.Persistence.Entities;

namespace evaluation_api.Infrastructure.Persistence.Mappers;

/// <summary>
/// Mapea entre Subproceso (dominio) y SubprocesoEntity (persistencia)
/// </summary>
public static class SubprocesoMapper
{
    public static SubprocesoEntity ToEntity(Subproceso domain)
    {
        return new SubprocesoEntity
        {
            IdSubproceso = domain.IdSubproceso ?? 0,
            IdProceso = domain.IdProceso,
            Nombre = domain.Nombre,
            Descripcion = domain.Descripcion
        };
    }

    public static Subproceso ToDomain(SubprocesoEntity entity)
    {
        return new Subproceso
        {
            IdSubproceso = entity.IdSubproceso,
            IdProceso = entity.IdProceso,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion
        };
    }
}
