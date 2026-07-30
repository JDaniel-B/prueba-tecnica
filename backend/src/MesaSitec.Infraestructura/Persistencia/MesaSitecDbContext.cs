using MesaSitec.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Persistencia;

public sealed class MesaSitecDbContext(DbContextOptions<MesaSitecDbContext> options)
    : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MesaSitecDbContext).Assembly);
    }
}
