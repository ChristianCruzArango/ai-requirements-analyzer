using Microsoft.EntityFrameworkCore;
using evaluation_api.Infrastructure.Persistence.Entities;

namespace evaluation_api.Infrastructure.Persistence;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos PostgreSQL
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ProcesoEntity> Procesos { get; set; }
    public DbSet<SubprocesoEntity> Subprocesos { get; set; }
    public DbSet<CasoUsoEntity> CasosUso { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProcesoEntity>(entity =>
        {
            entity.ToTable("proceso");
            entity.HasKey(e => e.IdProceso);
            entity.Property(e => e.IdProceso).HasColumnName("id_proceso");
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
        });

        modelBuilder.Entity<SubprocesoEntity>(entity =>
        {
            entity.ToTable("subproceso");
            entity.HasKey(e => e.IdSubproceso);
            entity.Property(e => e.IdSubproceso).HasColumnName("id_subproceso");
            entity.Property(e => e.IdProceso).HasColumnName("id_proceso");
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");

            entity.HasOne<ProcesoEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.IdProceso)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CasoUsoEntity>(entity =>
        {
            entity.ToTable("caso_uso");
            entity.HasKey(e => e.IdCasoUso);
            entity.Property(e => e.IdCasoUso).HasColumnName("id_caso_uso");
            entity.Property(e => e.IdSubproceso).HasColumnName("id_subproceso");
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.ActorPrincipal).HasColumnName("actor_principal").HasMaxLength(100);
            entity.Property(e => e.TipoCasoUso).HasColumnName("tipo_caso_uso");
            entity.Property(e => e.Precondiciones).HasColumnName("precondiciones");
            entity.Property(e => e.Postcondiciones).HasColumnName("postcondiciones");
            entity.Property(e => e.CriteriosDeAceptacion).HasColumnName("criterios_de_aceptacion");

            entity.HasOne<SubprocesoEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.IdSubproceso)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
