using MesaSitec.Aplicacion.Solicitudes.Modelos;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Infraestructura.Persistencia;
using MesaSitec.Infraestructura.Persistencia.Semillas;
using MesaSitec.Infraestructura.Solicitudes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.UnitTests.Solicitudes;

public sealed class SolicitudConsultaRepositoryTests
{
    private static readonly DateTime FechaBase =
        new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);
    private static readonly Guid TenantNorteId =
        Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid UsuarioNorte1Id =
        Guid.Parse("10000000-0000-0000-0000-000000000004");
    private static readonly Guid SolicitudNorte14Id =
        Guid.Parse("40000000-0000-0000-0000-000000000014");
    private static readonly Guid TenantSurId =
        Guid.Parse("00000000-0000-0000-0000-000000000002");

    [Fact]
    public async Task ObtenerDetalle_DevuelveCamposCompletosYFechasUtc()
    {
        await using var baseDatos = await BaseDatosSemilla.CrearAsync();
        var repositorio = new SolicitudConsultaRepository(baseDatos.Context);

        var resultado = await repositorio.ObtenerDetalleAsync(
            SolicitudNorte14Id,
            TenantNorteId,
            FechaBase);

        Assert.NotNull(resultado);
        Assert.Equal("SOL-2026-00014", resultado.Codigo);
        Assert.Contains("Descripción determinista", resultado.Descripcion);
        Assert.Equal(EstadoSolicitud.Resuelta, resultado.Estado);
        Assert.Equal(PrioridadSolicitud.Media, resultado.Prioridad);
        Assert.Equal("Requerimiento", resultado.Categoria.Nombre);
        Assert.Equal("Solicitante Norte Dos", resultado.Solicitante.Nombre);
        Assert.Equal("Agente Norte Dos", resultado.Agente?.Nombre);
        Assert.NotNull(resultado.FechaResolucion);
        Assert.NotNull(resultado.MotivoResolucion);
        Assert.Null(resultado.MotivoCancelacion);
        Assert.False(resultado.Vencida);
        Assert.Equal(DateTimeKind.Utc, resultado.FechaCreacion.Kind);
        Assert.Equal(DateTimeKind.Utc, resultado.FechaLimiteSla.Kind);
        Assert.Equal(DateTimeKind.Utc, resultado.FechaResolucion?.Kind);
    }

    [Fact]
    public async Task ObtenerDetalle_OtroTenantOIdInexistenteDevuelveNull()
    {
        await using var baseDatos = await BaseDatosSemilla.CrearAsync();
        var repositorio = new SolicitudConsultaRepository(baseDatos.Context);

        var otroTenant = await repositorio.ObtenerDetalleAsync(
            SolicitudNorte14Id,
            TenantSurId,
            FechaBase);
        var inexistente = await repositorio.ObtenerDetalleAsync(
            Guid.NewGuid(),
            TenantNorteId,
            FechaBase);

        Assert.Null(otroTenant);
        Assert.Null(inexistente);
    }

    [Fact]
    public async Task Listar_AislaTenantYPaginaEnElServidor()
    {
        await using var baseDatos = await BaseDatosSemilla.CrearAsync();
        var repositorio = new SolicitudConsultaRepository(baseDatos.Context);
        var filtro = CrearFiltro(page: 2, pageSize: 10);

        var resultado = await repositorio.ListarAsync(filtro);

        Assert.Equal(25, resultado.Total);
        Assert.Equal(3, resultado.TotalPaginas);
        Assert.Equal(10, resultado.Items.Count);
        Assert.Equal("SOL-2026-00011", resultado.Items[0].Codigo);
        Assert.All(
            resultado.Items,
            item =>
            {
                Assert.Contains("Norte", item.Titulo);
                Assert.Equal(DateTimeKind.Utc, item.FechaCreacion.Kind);
                Assert.Equal(DateTimeKind.Utc, item.FechaLimiteSla.Kind);
            });
    }

    [Fact]
    public async Task Listar_RestringeElSolicitanteASusPropiasSolicitudes()
    {
        await using var baseDatos = await BaseDatosSemilla.CrearAsync();
        var repositorio = new SolicitudConsultaRepository(baseDatos.Context);
        var filtro = CrearFiltro(
            solicitanteId: UsuarioNorte1Id,
            pageSize: 100);

        var resultado = await repositorio.ListarAsync(filtro);

        Assert.Equal(13, resultado.Total);
        Assert.All(
            resultado.Items,
            item =>
            {
                var correlativo = int.Parse(item.Codigo[^5..]);
                Assert.True(correlativo % 2 == 1);
            });
    }

    [Fact]
    public async Task Listar_PaginaPosteriorAlTotalDevuelveColeccionVacia()
    {
        await using var baseDatos = await BaseDatosSemilla.CrearAsync();
        var repositorio = new SolicitudConsultaRepository(baseDatos.Context);

        var resultado = await repositorio.ListarAsync(
            CrearFiltro(page: int.MaxValue));

        Assert.Equal(25, resultado.Total);
        Assert.Equal(2, resultado.TotalPaginas);
        Assert.Empty(resultado.Items);
    }

    [Fact]
    public async Task Listar_AplicaBusquedaLiteralSinDistinguirMayusculas()
    {
        await using var baseDatos = await BaseDatosSemilla.CrearAsync();
        var repositorio = new SolicitudConsultaRepository(baseDatos.Context);

        var encontrado = await repositorio.ListarAsync(
            CrearFiltro(busqueda: "NORTE 02"));
        var comodinEscapado = await repositorio.ListarAsync(
            CrearFiltro(busqueda: "%"));

        var item = Assert.Single(encontrado.Items);
        Assert.Equal("SOL-2026-00002", item.Codigo);
        Assert.Equal(0, comodinEscapado.Total);
    }

    [Fact]
    public async Task Listar_CombinaFiltrosExactos()
    {
        await using var baseDatos = await BaseDatosSemilla.CrearAsync();
        var repositorio = new SolicitudConsultaRepository(baseDatos.Context);
        var filtro = CrearFiltro(
            estado: EstadoSolicitud.EnProceso,
            prioridad: PrioridadSolicitud.Alta,
            categoriaId: Guid.Parse(
                "20000000-0000-0000-0000-000000000003"),
            agenteId: Guid.Parse(
                "10000000-0000-0000-0000-000000000002"));

        var resultado = await repositorio.ListarAsync(filtro);

        var item = Assert.Single(resultado.Items);
        Assert.Equal("SOL-2026-00011", item.Codigo);
        Assert.Equal(EstadoSolicitud.EnProceso, item.Estado);
        Assert.Equal(PrioridadSolicitud.Alta, item.Prioridad);
        Assert.Equal("Consulta", item.Categoria.Nombre);
        Assert.Equal("Agente Norte Uno", item.Agente?.Nombre);
    }

    [Fact]
    public async Task Listar_FiltraVencidasYCalculaLaMarcaConLaMismaRegla()
    {
        await using var baseDatos = await BaseDatosSemilla.CrearAsync();
        var repositorio = new SolicitudConsultaRepository(baseDatos.Context);

        var resultado = await repositorio.ListarAsync(
            CrearFiltro(vencidas: true, pageSize: 100));

        Assert.NotEmpty(resultado.Items);
        Assert.Equal(resultado.Total, resultado.Items.Count);
        Assert.All(resultado.Items, item => Assert.True(item.Vencida));
        Assert.DoesNotContain(
            resultado.Items,
            item => item.Estado is EstadoSolicitud.Resuelta
                or EstadoSolicitud.Cerrada
                or EstadoSolicitud.Cancelada);
    }

    [Fact]
    public async Task Listar_OrdenaPrioridadConSuValorSemantico()
    {
        await using var baseDatos = await BaseDatosSemilla.CrearAsync();
        var repositorio = new SolicitudConsultaRepository(baseDatos.Context);

        var resultado = await repositorio.ListarAsync(
            CrearFiltro(orden: "-prioridad", pageSize: 100));
        var prioridades = resultado.Items
            .Select(item => item.Prioridad)
            .ToArray();

        Assert.Equal(PrioridadSolicitud.Critica, prioridades[0]);
        Assert.Equal(PrioridadSolicitud.Baja, prioridades[^1]);
        Assert.Equal(
            prioridades.OrderByDescending(prioridad => prioridad),
            prioridades);
    }

    private static FiltroSolicitudes CrearFiltro(
        Guid? solicitanteId = null,
        EstadoSolicitud? estado = null,
        PrioridadSolicitud? prioridad = null,
        Guid? categoriaId = null,
        Guid? agenteId = null,
        string? busqueda = null,
        bool? vencidas = null,
        int page = 1,
        int pageSize = 20,
        string orden = "-fechaCreacion")
    {
        return new FiltroSolicitudes(
            TenantNorteId,
            solicitanteId,
            estado,
            prioridad,
            categoriaId,
            agenteId,
            busqueda,
            vencidas,
            FechaBase,
            page,
            pageSize,
            orden);
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
