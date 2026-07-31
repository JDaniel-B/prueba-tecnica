using MesaSitec.Aplicacion.Excepciones;
using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Aplicacion.Solicitudes.Modelos;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.UnitTests.Solicitudes;

public sealed class SolicitudActualizacionServiceTests
{
    private static readonly DateTimeOffset Ahora =
        new(2026, 2, 1, 10, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(RolUsuario.Admin)]
    [InlineData(RolUsuario.Agente)]
    [InlineData(RolUsuario.Solicitante)]
    public async Task Actualizar_RolesPermitidosGuardanYDevuelvenDetalle(
        RolUsuario rol)
    {
        var tenantId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var solicitud = CrearSolicitud(tenantId, usuarioId);
        var categoriaId = Guid.NewGuid();
        var escrituraRepository = new EscrituraRepositoryStub(
            solicitud,
            new CategoriaParaSolicitud(categoriaId, 8));
        var consultaRepository = new ConsultaRepositoryStub(
            id => CrearDetalle(id, solicitud, usuarioId, categoriaId));
        var servicio = new SolicitudActualizacionService(
            escrituraRepository,
            consultaRepository,
            new RelojFijo(Ahora));
        var request = CrearRequest(categoriaId);

        var resultado = await servicio.ActualizarAsync(
            solicitud.Id,
            request,
            tenantId,
            usuarioId,
            rol);

        Assert.Equal("Título actualizado de solicitud", solicitud.Titulo);
        Assert.Equal(PrioridadSolicitud.Critica, solicitud.Prioridad);
        Assert.Equal(categoriaId, solicitud.CategoriaId);
        Assert.Equal(1, escrituraRepository.Guardados);
        Assert.Equal(solicitud.Id, resultado.Id);
        Assert.Equal(Ahora.UtcDateTime, consultaRepository.UltimoAhoraUtc);
    }

    [Fact]
    public async Task Actualizar_SolicitanteNoPuedeEditarSolicitudAjena()
    {
        var solicitud = CrearSolicitud(Guid.NewGuid(), Guid.NewGuid());
        var servicio = CrearServicio(solicitud);

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(
            () => servicio.ActualizarAsync(
                solicitud.Id,
                CrearRequest(Guid.NewGuid()),
                solicitud.TenantId,
                Guid.NewGuid(),
                RolUsuario.Solicitante));
    }

    [Fact]
    public async Task Actualizar_SolicitanteNoPuedeEditarSiYaNoEstaNueva()
    {
        var usuarioId = Guid.NewGuid();
        var solicitud = CrearSolicitud(Guid.NewGuid(), usuarioId);
        EstablecerEstado(solicitud, EstadoSolicitud.Asignada);
        var servicio = CrearServicio(solicitud);

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(
            () => servicio.ActualizarAsync(
                solicitud.Id,
                CrearRequest(Guid.NewGuid()),
                solicitud.TenantId,
                usuarioId,
                RolUsuario.Solicitante));
    }

    [Fact]
    public async Task Actualizar_OtroTenantOIdInexistenteDevuelveNoEncontrado()
    {
        var servicio = CrearServicio(null);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => servicio.ActualizarAsync(
                Guid.NewGuid(),
                CrearRequest(Guid.NewGuid()),
                Guid.NewGuid(),
                Guid.NewGuid(),
                RolUsuario.Admin));
    }

    [Fact]
    public async Task Actualizar_RechazaCategoriaInactivaODeOtroTenant()
    {
        var solicitud = CrearSolicitud(Guid.NewGuid(), Guid.NewGuid());
        var escrituraRepository = new EscrituraRepositoryStub(solicitud, null);
        var servicio = new SolicitudActualizacionService(
            escrituraRepository,
            new ConsultaRepositoryStub(_ => null),
            new RelojFijo(Ahora));

        var exception = await Assert.ThrowsAsync<ValidacionException>(
            () => servicio.ActualizarAsync(
                solicitud.Id,
                CrearRequest(Guid.NewGuid()),
                solicitud.TenantId,
                Guid.NewGuid(),
                RolUsuario.Admin));

        Assert.Contains("categoriaId", exception.Errores.Keys);
        Assert.Equal(0, escrituraRepository.Guardados);
    }

    private static SolicitudActualizacionService CrearServicio(
        Solicitud? solicitud)
    {
        return new SolicitudActualizacionService(
            new EscrituraRepositoryStub(
                solicitud,
                new CategoriaParaSolicitud(Guid.NewGuid(), 8)),
            new ConsultaRepositoryStub(_ => null),
            new RelojFijo(Ahora));
    }

    private static SolicitudEscrituraRequest CrearRequest(Guid categoriaId)
    {
        return new SolicitudEscrituraRequest
        {
            Titulo = "Título actualizado de solicitud",
            Descripcion = "Descripción actualizada con suficiente información para soporte.",
            CategoriaId = categoriaId,
            Prioridad = PrioridadSolicitud.Critica
        };
    }

    private static Solicitud CrearSolicitud(Guid tenantId, Guid solicitanteId)
    {
        return new Solicitud(
            Guid.NewGuid(),
            tenantId,
            "SOL-2026-00001",
            "No puedo acceder al portal",
            "El portal rechaza mis credenciales de acceso.",
            Guid.NewGuid(),
            PrioridadSolicitud.Alta,
            solicitanteId,
            Ahora.UtcDateTime,
            8);
    }

    private static SolicitudDetalleResponse CrearDetalle(
        Guid id,
        Solicitud solicitud,
        Guid solicitanteId,
        Guid categoriaId)
    {
        return new SolicitudDetalleResponse(
            id,
            solicitud.Codigo,
            solicitud.Titulo,
            solicitud.Descripcion,
            solicitud.Estado,
            solicitud.Prioridad,
            new CategoriaResumenResponse(categoriaId, "Incidente"),
            new UsuarioResumenResponse(solicitanteId, "Solicitante"),
            null,
            solicitud.FechaCreacion,
            solicitud.FechaLimiteSla,
            null,
            null,
            null,
            false);
    }

    private static void EstablecerEstado(
        Solicitud solicitud,
        EstadoSolicitud estado)
    {
        typeof(Solicitud)
            .GetProperty(nameof(Solicitud.Estado))!
            .SetValue(solicitud, estado);
    }

    private sealed class EscrituraRepositoryStub(
        Solicitud? solicitud,
        CategoriaParaSolicitud? categoria) : ISolicitudEscrituraRepository
    {
        public int Guardados { get; private set; }

        public Task<CategoriaParaSolicitud?> BuscarCategoriaActivaAsync(
            Guid categoriaId,
            Guid tenantId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(categoria);
        }

        public Task<Solicitud?> BuscarAsync(
            Guid id,
            Guid tenantId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(solicitud);
        }

        public Task<int> ObtenerSiguienteCorrelativoAsync(
            Guid tenantId,
            int anio,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task AgregarAsync(
            Solicitud solicitud,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task GuardarCambiosAsync(
            CancellationToken cancellationToken = default)
        {
            Guardados++;
            return Task.CompletedTask;
        }
    }

    private sealed class ConsultaRepositoryStub(
        Func<Guid, SolicitudDetalleResponse?> detalleFactory)
        : ISolicitudConsultaRepository
    {
        public DateTime? UltimoAhoraUtc { get; private set; }

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
            UltimoAhoraUtc = ahoraUtc;
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
