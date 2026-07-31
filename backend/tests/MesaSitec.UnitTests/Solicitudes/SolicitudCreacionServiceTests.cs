using MesaSitec.Aplicacion.Excepciones;
using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Aplicacion.Solicitudes.Modelos;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.UnitTests.Solicitudes;

public sealed class SolicitudCreacionServiceTests
{
    private static readonly DateTimeOffset Ahora =
        new(2026, 2, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Crear_UsaDatosDelServidorYCalculaCodigoYSla()
    {
        var categoriaId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var creacionRepository = new CreacionRepositoryStub(
            new CategoriaParaSolicitud(categoriaId, 8),
            correlativo: 26);
        var consultaRepository = new ConsultaRepositoryStub(
            id =>
            {
                var solicitud = creacionRepository.Guardada!;
                return new SolicitudDetalleResponse(
                    id,
                    solicitud.Codigo,
                    solicitud.Titulo,
                    solicitud.Descripcion,
                    solicitud.Estado,
                    solicitud.Prioridad,
                    new CategoriaResumenResponse(categoriaId, "Incidente"),
                    new UsuarioResumenResponse(usuarioId, "Solicitante"),
                    null,
                    solicitud.FechaCreacion,
                    solicitud.FechaLimiteSla,
                    null,
                    null,
                    null,
                    false);
            });
        var servicio = new SolicitudCreacionService(
            creacionRepository,
            consultaRepository,
            new RelojFijo(Ahora));
        var request = CrearRequest(
            categoriaId,
            PrioridadSolicitud.Critica);

        var resultado = await servicio.CrearAsync(
            request,
            tenantId,
            usuarioId);

        var guardada = Assert.IsType<Solicitud>(creacionRepository.Guardada);
        Assert.Equal(tenantId, guardada.TenantId);
        Assert.Equal(usuarioId, guardada.SolicitanteId);
        Assert.Equal("SOL-2026-00026", guardada.Codigo);
        Assert.Equal(EstadoSolicitud.Nueva, guardada.Estado);
        Assert.Null(guardada.AgenteId);
        Assert.Equal(Ahora.UtcDateTime, guardada.FechaCreacion);
        Assert.Equal(Ahora.AddHours(4).UtcDateTime, guardada.FechaLimiteSla);
        Assert.Equal(guardada.Id, resultado.Id);
        Assert.Equal(guardada.Id, consultaRepository.UltimoId);
        Assert.Equal(tenantId, consultaRepository.UltimoTenantId);
    }

    [Fact]
    public async Task Crear_RechazaCategoriaInactivaInexistenteODeOtroTenant()
    {
        var servicio = new SolicitudCreacionService(
            new CreacionRepositoryStub(null, 1),
            new ConsultaRepositoryStub(_ => null),
            new RelojFijo(Ahora));

        var exception = await Assert.ThrowsAsync<ValidacionException>(
            () => servicio.CrearAsync(
                CrearRequest(Guid.NewGuid(), PrioridadSolicitud.Alta),
                Guid.NewGuid(),
                Guid.NewGuid()));

        Assert.Contains("categoriaId", exception.Errores.Keys);
    }

    [Fact]
    public async Task Crear_AgrupaErroresDeTodosLosCamposInvalidos()
    {
        var creacionRepository = new CreacionRepositoryStub(null, 1);
        var servicio = new SolicitudCreacionService(
            creacionRepository,
            new ConsultaRepositoryStub(_ => null),
            new RelojFijo(Ahora));
        var request = new SolicitudEscrituraRequest
        {
            Titulo = "1234",
            Descripcion = "corta",
            CategoriaId = Guid.Empty,
            Prioridad = (PrioridadSolicitud)99
        };

        var exception = await Assert.ThrowsAsync<ValidacionException>(
            () => servicio.CrearAsync(
                request,
                Guid.NewGuid(),
                Guid.NewGuid()));

        Assert.Equal(
            ["categoriaId", "descripcion", "prioridad", "titulo"],
            exception.Errores.Keys.OrderBy(campo => campo));
        Assert.Null(creacionRepository.Guardada);
    }

    [Fact]
    public async Task Crear_ValidaLongitudesDespuesDeEliminarEspacios()
    {
        var creacionRepository = new CreacionRepositoryStub(null, 1);
        var servicio = new SolicitudCreacionService(
            creacionRepository,
            new ConsultaRepositoryStub(_ => null),
            new RelojFijo(Ahora));
        var request = new SolicitudEscrituraRequest
        {
            Titulo = "    x    ",
            Descripcion = "         x         ",
            CategoriaId = Guid.NewGuid(),
            Prioridad = PrioridadSolicitud.Media
        };

        var exception = await Assert.ThrowsAsync<ValidacionException>(
            () => servicio.CrearAsync(
                request,
                Guid.NewGuid(),
                Guid.NewGuid()));

        Assert.Contains("titulo", exception.Errores.Keys);
        Assert.Contains("descripcion", exception.Errores.Keys);
        Assert.Null(creacionRepository.Guardada);
    }

    private static SolicitudEscrituraRequest CrearRequest(
        Guid categoriaId,
        PrioridadSolicitud prioridad)
    {
        return new SolicitudEscrituraRequest
        {
            Titulo = "No puedo acceder al portal",
            Descripcion = "El portal rechaza mis credenciales de acceso.",
            CategoriaId = categoriaId,
            Prioridad = prioridad
        };
    }

    private sealed class CreacionRepositoryStub(
        CategoriaParaSolicitud? categoria,
        int correlativo) : ISolicitudEscrituraRepository
    {
        public Solicitud? Guardada { get; private set; }

        public Task<CategoriaParaSolicitud?> BuscarCategoriaActivaAsync(
            Guid categoriaId,
            Guid tenantId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(categoria);
        }

        public Task<int> ObtenerSiguienteCorrelativoAsync(
            Guid tenantId,
            int anio,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(correlativo);
        }

        public Task<Solicitud?> BuscarAsync(
            Guid id,
            Guid tenantId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Solicitud?>(null);
        }

        public Task AgregarAsync(
            Solicitud solicitud,
            CancellationToken cancellationToken = default)
        {
            Guardada = solicitud;
            return Task.CompletedTask;
        }

        public Task GuardarCambiosAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class ConsultaRepositoryStub(
        Func<Guid, SolicitudDetalleResponse?> detalleFactory)
        : ISolicitudConsultaRepository
    {
        public Guid? UltimoId { get; private set; }

        public Guid? UltimoTenantId { get; private set; }

        public Task<PaginaResponse<SolicitudListadoResponse>> ListarAsync(
            FiltroSolicitudes filtro,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SolicitudDetalleResponse?> ObtenerDetalleAsync(
            Guid id,
            Guid tenantId,
            DateTime ahoraUtc,
            CancellationToken cancellationToken = default)
        {
            UltimoId = id;
            UltimoTenantId = tenantId;
            return Task.FromResult(detalleFactory(id));
        }
    }

    private sealed class RelojFijo(DateTimeOffset ahora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return ahora;
        }
    }
}
