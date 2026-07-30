using MesaSitec.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MesaSitec.Infraestructura.Persistencia.Configuraciones;

public sealed class CategoriaConfiguracion : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable(
            "Categorias",
            table => table.HasCheckConstraint(
                "CK_Categorias_SlaHoras_Positivo",
                "\"SlaHoras\" > 0"));

        builder.HasKey(categoria => categoria.Id);
        builder.HasAlternateKey(categoria => new { categoria.TenantId, categoria.Id });

        builder.Property(categoria => categoria.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(categoria => categoria.SlaHoras)
            .IsRequired();

        builder.Property(categoria => categoria.Activo)
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(categoria => categoria.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(categoria => new { categoria.TenantId, categoria.Activo });
    }
}
