using MesaSitec.Dominio.Entidades;
using MesaSitec.Infraestructura.Persistencia;
using MesaSitec.Infraestructura.Persistencia.Semillas;
using MesaSitec.Infraestructura.Solicitudes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.UnitTests.Solicitudes;

public sealed class SolicitudCreacionRepositoryTests
{
    private static readonly DateTime FechaBase =
        new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);
    private static readonly Guid TenantNorteId =
        Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid TenantSurId =
        Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid CategoriaNorteId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid CategoriaSurId =
        Guid.Parse("30000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task BuscarCategoriaActiva_AislaTenantYEstado()
    {
        await using var baseDatos = await BaseDatosSemilla.CrearAsync();
        var categoriaInactiva = new Categoria(
            Guid.NewGuid(),
            TenantNorteId,
            "Categoría inactiva",
            12,
            activo: false);
        baseDatos.Context.Categorias.Add(categoriaInactiva);
        await baseDatos.Context.SaveChangesAsync();
        var repositorio = new SolicitudCreacionRepository(baseDatos.Context);

        var activa = await repositorio.BuscarCategoriaActivaAsync(
            CategoriaNorteId,
            TenantNorteId);
        var otroTenant = await repositorio.BuscarCategoriaActivaAsync(
            CategoriaSurId,
            TenantNorteId);
        var inactiva = await repositorio.BuscarCategoriaActivaAsync(
            categoriaInactiva.Id,
            TenantNorteId);

        Assert.NotNull(activa);
        Assert.Equal(8, activa.SlaHoras);
        Assert.Null(otroTenant);
        Assert.Null(inactiva);
    }

    [Fact]
    public async Task ObtenerSiguienteCorrelativo_EsIndependientePorTenantYAnio()
    {
        await using var baseDatos = await BaseDatosSemilla.CrearAsync();
        var repositorio = new SolicitudCreacionRepository(baseDatos.Context);

        var siguienteNorte = await repositorio.ObtenerSiguienteCorrelativoAsync(
            TenantNorteId,
            2026);
        var siguienteSur = await repositorio.ObtenerSiguienteCorrelativoAsync(
            TenantSurId,
            2026);
        var primerSiguienteAnio =
            await repositorio.ObtenerSiguienteCorrelativoAsync(
                TenantNorteId,
                2027);

        Assert.Equal(26, siguienteNorte);
        Assert.Equal(9, siguienteSur);
        Assert.Equal(1, primerSiguienteAnio);
    }

    private sealed class BaseDatosSemilla : IAsyncDisposable
    {
        private BaseDatosSemilla(
            SqliteConnection connection,
            MesaSitecDbContext context)
        {
            Connection = connection;
            Context = context;
        }

        public SqliteConnection Connection { get; }

        public MesaSitecDbContext Context { get; }

        public static async Task<BaseDatosSemilla> CrearAsync()
        {
            var connection = new SqliteConnection(
                "Data Source=:memory:;Foreign Keys=True");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<MesaSitecDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new MesaSitecDbContext(options);
            await context.Database.MigrateAsync();
            var sembrador = new SembradorDatos(
                context,
                new PasswordHasher<Usuario>());
            await sembrador.SembrarAsync(FechaBase);

            return new BaseDatosSemilla(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
