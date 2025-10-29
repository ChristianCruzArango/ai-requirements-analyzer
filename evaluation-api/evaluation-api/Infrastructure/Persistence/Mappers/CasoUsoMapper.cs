using evaluation_api.Domain.Enums;
using evaluation_api.Domain.Models;
using evaluation_api.Infrastructure.Persistence.Entities;

namespace evaluation_api.Infrastructure.Persistence.Mappers;

/// <summary>
/// Mapea entre CasoUso (dominio) y CasoUsoEntity (persistencia)
/// </summary>
public static class CasoUsoMapper
{
    public static CasoUsoEntity ToEntity(CasoUso domain)
    {
        return new CasoUsoEntity
        {
            IdCasoUso = domain.IdCasoUso ?? 0,
            IdSubproceso = domain.IdSubproceso,
            Nombre = domain.Nombre,
            Descripcion = domain.Descripcion,
            ActorPrincipal = domain.ActorPrincipal,
            TipoCasoUso = (int)domain.TipoCasoUso,
            Precondiciones = domain.Precondiciones,
            Postcondiciones = domain.Postcondiciones,
            CriteriosDeAceptacion = domain.CriteriosDeAceptacion
        };
    }

    public static CasoUso ToDomain(CasoUsoEntity entity)
    {
        return new CasoUso
        {
            IdCasoUso = entity.IdCasoUso,
            IdSubproceso = entity.IdSubproceso,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            ActorPrincipal = entity.ActorPrincipal,
            TipoCasoUso = (TipoCasoUso)entity.TipoCasoUso,
            Precondiciones = entity.Precondiciones,
            Postcondiciones = entity.Postcondiciones,
            CriteriosDeAceptacion = entity.CriteriosDeAceptacion
        };
    }
}
