using MesaSitec.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MesaSitec.Infraestructura.Persistencia.Configuraciones;

public sealed class SolicitudConfiguracion : IEntityTypeConfiguration<Solicitud>
{
    public void Configure(EntityTypeBuilder<Solicitud> builder)
    {
        var conversorFechaUtc = new ValueConverter<DateTime, DateTime>(
            fecha => fecha,
            fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));

        builder.ToTable(
            "Solicitudes",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Solicitudes_Titulo_Longitud",
                    "length(trim(\"Titulo\")) BETWEEN 5 AND 120");
                table.HasCheckConstraint(
                    "CK_Solicitudes_Descripcion_Longitud",
                    "length(trim(\"Descripcion\")) BETWEEN 10 AND 4000");
            });

        builder.HasKey(solicitud => solicitud.Id);

        builder.Property(solicitud => solicitud.Codigo)
            .HasMaxLength(14)
            .IsRequired();

        builder.HasIndex(solicitud => new { solicitud.TenantId, solicitud.Codigo })
            .IsUnique();

        builder.Property(solicitud => solicitud.Titulo)
            .HasMaxLength(Solicitud.TituloLongitudMaxima)
            .IsRequired();

        builder.Property(solicitud => solicitud.Descripcion)
            .HasMaxLength(Solicitud.DescripcionLongitudMaxima)
            .IsRequired();

        builder.Property(solicitud => solicitud.Prioridad)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(solicitud => solicitud.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(solicitud => solicitud.FechaCreacion)
            .HasConversion(conversorFechaUtc)
            .IsRequired();

        builder.Property(solicitud => solicitud.FechaLimiteSla)
            .HasConversion(conversorFechaUtc)
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(solicitud => solicitud.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(solicitud => new { solicitud.TenantId, solicitud.CategoriaId })
            .HasPrincipalKey(categoria => new { categoria.TenantId, categoria.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(solicitud => new { solicitud.TenantId, solicitud.SolicitanteId })
            .HasPrincipalKey(usuario => new { usuario.TenantId, usuario.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(solicitud => new { solicitud.TenantId, solicitud.AgenteId })
            .HasPrincipalKey(usuario => new { usuario.TenantId, usuario.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(solicitud => new { solicitud.TenantId, solicitud.Estado });
        builder.HasIndex(solicitud => new { solicitud.TenantId, solicitud.Prioridad });
        builder.HasIndex(solicitud => new { solicitud.TenantId, solicitud.CategoriaId });
        builder.HasIndex(solicitud => new { solicitud.TenantId, solicitud.AgenteId });
        builder.HasIndex(solicitud => new { solicitud.TenantId, solicitud.FechaCreacion });
    }
}
