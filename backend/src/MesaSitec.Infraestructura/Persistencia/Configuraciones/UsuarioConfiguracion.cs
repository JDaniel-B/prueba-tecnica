using MesaSitec.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MesaSitec.Infraestructura.Persistencia.Configuraciones;

public sealed class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(usuario => usuario.Id);
        builder.HasAlternateKey(usuario => new { usuario.TenantId, usuario.Id });

        builder.Property(usuario => usuario.Email)
            .HasMaxLength(320)
            .UseCollation("NOCASE")
            .IsRequired();

        builder.HasIndex(usuario => usuario.Email)
            .IsUnique();

        builder.Property(usuario => usuario.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(usuario => usuario.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(usuario => usuario.Rol)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(usuario => usuario.Activo)
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(usuario => usuario.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(usuario => new { usuario.TenantId, usuario.Rol, usuario.Activo });
    }
}
