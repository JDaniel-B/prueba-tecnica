using MesaSitec.Dominio.Entidades;
using MesaSitec.Infraestructura.Categorias;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.UnitTests.Categorias;

public sealed class CategoriaConsultaServiceTests
{
    [Fact]
    public async Task ListarActivas_FiltraTenantEInactivasYOrdenaPorNombre()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MesaSitecDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new MesaSitecDbContext(options);
        await context.Database.EnsureCreatedAsync();
        var tenantNorte = new Tenant(Guid.NewGuid(), "Cooperativa Norte");
        var tenantSur = new Tenant(Guid.NewGuid(), "Bufete Sur");
        context.AddRange(
            tenantNorte,
            tenantSur,
            new Categoria(
                Guid.NewGuid(),
                tenantNorte.Id,
                "Requerimiento",
                40),
            new Categoria(
                Guid.NewGuid(),
                tenantNorte.Id,
                "Incidente",
                8),
            new Categoria(
                Guid.NewGuid(),
                tenantNorte.Id,
                "Categoría inactiva",
                12,
                activo: false),
            new Categoria(
                Guid.NewGuid(),
                tenantSur.Id,
                "Categoría de Sur",
                24));
        await context.SaveChangesAsync();
        var servicio = new CategoriaConsultaService(context);

        var resultado = await servicio.ListarActivasAsync(tenantNorte.Id);

        Assert.Collection(
            resultado,
            categoria =>
            {
                Assert.Equal("Incidente", categoria.Nombre);
                Assert.Equal(8, categoria.SlaHoras);
            },
            categoria =>
            {
                Assert.Equal("Requerimiento", categoria.Nombre);
                Assert.Equal(40, categoria.SlaHoras);
            });
    }
}
