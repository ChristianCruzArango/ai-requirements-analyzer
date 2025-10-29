using evaluation_api.Domain.Models;
using evaluation_api.Infrastructure.Persistence.Entities;

namespace evaluation_api.Infrastructure.Persistence.Mappers;

/// <summary>
/// Mapea entre Proceso (dominio) y ProcesoEntity (persistencia)
/// </summary>
public static class ProcesoMapper
{
    public static ProcesoEntity ToEntity(Proceso domain)
    {
        return new ProcesoEntity
        {
            IdProceso = domain.IdProceso ?? 0,
            Nombre = domain.Nombre,
            Descripcion = domain.Descripcion
        };
    }

    public static Proceso ToDomain(ProcesoEntity entity)
    {
        return new Proceso
        {
            IdProceso = entity.IdProceso,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion
        };
    }
}
