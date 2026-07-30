using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.UnitTests.Persistencia;

public sealed class MesaSitecDbContextTests
{
    private static readonly DateTime FechaBase =
        new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task MigracionInicial_CreaLasCuatroTablas()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MesaSitecDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new MesaSitecDbContext(options);

        await context.Database.MigrateAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name IN ('Tenants', 'Usuarios', 'Categorias', 'Solicitudes');
            """;
        var cantidadTablas = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(4, cantidadTablas);
    }

    [Fact]
    public async Task DbContext_PersisteLasCuatroEntidades()
    {
        await using var baseDatos = await BaseDatosEnMemoria.CrearAsync();
        var tenant = new Tenant(Guid.NewGuid(), "Cooperativa Norte");
        var usuario = CrearUsuario(tenant.Id, "user1@norte.test");
        var categoria = new Categoria(
            Guid.NewGuid(),
            tenant.Id,
            "Incidente",
            8);
        var solicitud = CrearSolicitud(tenant.Id, categoria.Id, usuario.Id);

        baseDatos.Context.AddRange(tenant, usuario, categoria, solicitud);
        await baseDatos.Context.SaveChangesAsync();

        Assert.Equal(1, await baseDatos.Context.Tenants.CountAsync());
        Assert.Equal(1, await baseDatos.Context.Usuarios.CountAsync());
        Assert.Equal(1, await baseDatos.Context.Categorias.CountAsync());
        Assert.Equal(1, await baseDatos.Context.Solicitudes.CountAsync());
    }

    [Fact]
    public async Task DbContext_ExigeEmailUnicoSinDistinguirMayusculas()
    {
        await using var baseDatos = await BaseDatosEnMemoria.CrearAsync();
        var norte = new Tenant(Guid.NewGuid(), "Cooperativa Norte");
        var sur = new Tenant(Guid.NewGuid(), "Bufete Sur");

        baseDatos.Context.AddRange(
            norte,
            sur,
            CrearUsuario(norte.Id, "admin@norte.test"),
            CrearUsuario(sur.Id, "ADMIN@NORTE.TEST"));

        await Assert.ThrowsAsync<DbUpdateException>(
            () => baseDatos.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task DbContext_ImpideRelacionarDatosDeTenantsDistintos()
    {
        await using var baseDatos = await BaseDatosEnMemoria.CrearAsync();
        var norte = new Tenant(Guid.NewGuid(), "Cooperativa Norte");
        var sur = new Tenant(Guid.NewGuid(), "Bufete Sur");
        var solicitanteNorte = CrearUsuario(norte.Id, "user1@norte.test");
        var categoriaSur = new Categoria(
            Guid.NewGuid(),
            sur.Id,
            "Incidente",
            8);
        var solicitudInvalida = CrearSolicitud(
            norte.Id,
            categoriaSur.Id,
            solicitanteNorte.Id);

        baseDatos.Context.AddRange(
            norte,
            sur,
            solicitanteNorte,
            categoriaSur,
            solicitudInvalida);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => baseDatos.Context.SaveChangesAsync());
    }

    private static Usuario CrearUsuario(Guid tenantId, string email)
    {
        return new Usuario(
            Guid.NewGuid(),
            tenantId,
            email,
            "hash-de-prueba",
            "Usuario de prueba",
            RolUsuario.Solicitante);
    }

    private static Solicitud CrearSolicitud(
        Guid tenantId,
        Guid categoriaId,
        Guid solicitanteId)
    {
        return new Solicitud(
            Guid.NewGuid(),
            tenantId,
            "SOL-2026-00001",
            "No puedo acceder al portal",
            "El portal rechaza mis credenciales de acceso.",
            categoriaId,
            PrioridadSolicitud.Alta,
            solicitanteId,
            FechaBase,
            8);
    }

    private sealed class BaseDatosEnMemoria : IAsyncDisposable
    {
        private BaseDatosEnMemoria(
            SqliteConnection connection,
            MesaSitecDbContext context)
        {
            Connection = connection;
            Context = context;
        }

        public SqliteConnection Connection { get; }

        public MesaSitecDbContext Context { get; }

        public static async Task<BaseDatosEnMemoria> CrearAsync()
        {
            var connection = new SqliteConnection(
                "Data Source=:memory:;Foreign Keys=True");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<MesaSitecDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new MesaSitecDbContext(options);
            await context.Database.EnsureCreatedAsync();

            return new BaseDatosEnMemoria(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
