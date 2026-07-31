using MesaSitec.Aplicacion.Excepciones;
using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Aplicacion.Solicitudes.Modelos;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.UnitTests.Solicitudes;

public sealed class SolicitudTransicionServiceTests
{
    private static readonly DateTimeOffset Ahora =
        new(2026, 2, 3, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(RolUsuario.Solicitante, "asignar")]
    [InlineData(RolUsuario.Solicitante, "iniciar")]
    [InlineData(RolUsuario.Solicitante, "resolver")]
    [InlineData(RolUsuario.Solicitante, "reabrir")]
    [InlineData(RolUsuario.Solicitante, "cancelar")]
    [InlineData(RolUsuario.Agente, "cancelar")]
    public async Task Transicionar_RechazaAccionesSinPermiso(
        RolUsuario rol,
        string accion)
    {
        var solicitud = CrearSolicitud();
        var contexto = CrearContexto(solicitud);

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(
            () => contexto.Servicio.TransicionarAsync(
                solicitud.Id,
                new TransicionarSolicitudRequest { Accion = accion },
                solicitud.TenantId,
                solicitud.SolicitanteId,
                rol));

        Assert.Equal(0, contexto.Escritura.Guardados);
    }

    [Fact]
    public async Task Cerrar_SolicitantePuedeCerrarSolicitudPropiaResuelta()
    {
        var solicitud = CrearSolicitud();
        solicitud.Asignar(Guid.NewGuid());
        solicitud.Iniciar();
        solicitud.Resolver(
            "La solución fue aplicada y confirmada por el usuario final.",
            Ahora.UtcDateTime);
        var contexto = CrearContexto(solicitud);

        var resultado = await contexto.Servicio.TransicionarAsync(
            solicitud.Id,
            new TransicionarSolicitudRequest { Accion = "cerrar" },
            solicitud.TenantId,
            solicitud.SolicitanteId,
            RolUsuario.Solicitante);

        Assert.Equal(EstadoSolicitud.Cerrada, resultado.Estado);
        Assert.Equal(1, contexto.Escritura.Guardados);
    }

    [Fact]
    public async Task Cerrar_SolicitanteNoPuedeCerrarSolicitudAjena()
    {
        var solicitud = CrearSolicitud();
        var contexto = CrearContexto(solicitud);

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(
            () => contexto.Servicio.TransicionarAsync(
                solicitud.Id,
                new TransicionarSolicitudRequest { Accion = "cerrar" },
                solicitud.TenantId,
                Guid.NewGuid(),
                RolUsuario.Solicitante));
    }

    [Fact]
    public async Task Asignar_RechazaAgenteInvalidoSinGuardar()
    {
        var solicitud = CrearSolicitud();
        var contexto = CrearContexto(solicitud, agenteValido: false);

        await Assert.ThrowsAsync<AgenteInvalidoException>(
            () => contexto.Servicio.TransicionarAsync(
                solicitud.Id,
                new TransicionarSolicitudRequest
                {
                    Accion = "asignar",
                    AgenteId = Guid.NewGuid()
                },
                solicitud.TenantId,
                Guid.NewGuid(),
                RolUsuario.Admin));

        Assert.Equal(0, contexto.Escritura.Guardados);
    }

    [Theory]
    [InlineData("resolver", "motivo corto")]
    [InlineData("cancelar", "corto")]
    public async Task Transicionar_RechazaMotivoCorto(
        string accion,
        string motivo)
    {
        var solicitud = CrearSolicitud();
        solicitud.Asignar(Guid.NewGuid());
        solicitud.Iniciar();
        var contexto = CrearContexto(solicitud);

        await Assert.ThrowsAsync<MotivoRequeridoException>(
            () => contexto.Servicio.TransicionarAsync(
                solicitud.Id,
                new TransicionarSolicitudRequest
                {
                    Accion = accion,
                    Motivo = motivo
                },
                solicitud.TenantId,
                Guid.NewGuid(),
                RolUsuario.Admin));
    }

    [Fact]
    public async Task Transicionar_RecursoDeOtroTenantSeTrataComoInexistente()
    {
        var contexto = CrearContexto(null);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => contexto.Servicio.TransicionarAsync(
                Guid.NewGuid(),
                new TransicionarSolicitudRequest { Accion = "cancelar" },
                Guid.NewGuid(),
                Guid.NewGuid(),
                RolUsuario.Admin));
    }

    private static Contexto CrearContexto(
        Solicitud? solicitud,
        bool agenteValido = true)
    {
        var escritura = new EscrituraRepositoryStub(solicitud);
        var servicio = new SolicitudTransicionService(
            escritura,
            new AgenteRepositoryStub(agenteValido),
            new ConsultaRepositoryStub(solicitud),
            new RelojFijo(Ahora));
        return new Contexto(servicio, escritura);
    }

    private static Solicitud CrearSolicitud()
    {
        return new Solicitud(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SOL-2026-00001",
            "No puedo acceder al portal",
            "El portal rechaza mis credenciales de acceso.",
            Guid.NewGuid(),
            PrioridadSolicitud.Alta,
            Guid.NewGuid(),
            Ahora.UtcDateTime.AddHours(-2),
            8);
    }

    private sealed record Contexto(
        SolicitudTransicionService Servicio,
        EscrituraRepositoryStub Escritura);

    private sealed class EscrituraRepositoryStub(Solicitud? solicitud)
        : ISolicitudEscrituraRepository
    {
        public int Guardados { get; private set; }
        public Task<Solicitud?> BuscarAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult(solicitud);
        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) { Guardados++; return Task.CompletedTask; }
        public Task<CategoriaParaSolicitud?> BuscarCategoriaActivaAsync(Guid categoriaId, Guid tenantId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> ObtenerSiguienteCorrelativoAsync(Guid tenantId, int anio, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AgregarAsync(Solicitud solicitud, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class AgenteRepositoryStub(bool valido) : IAgenteRepository
    {
        public Task<bool> EsValidoAsync(Guid agenteId, Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult(valido);
        public Task<IReadOnlyList<UsuarioResumenResponse>> ListarActivosAsync(Guid tenantId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class ConsultaRepositoryStub(Solicitud? solicitud)
        : ISolicitudConsultaRepository
    {
        public Task<PaginaResponse<SolicitudListadoResponse>> ListarAsync(FiltroSolicitudes filtro, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<SolicitudDetalleResponse?> ObtenerDetalleAsync(Guid id, Guid tenantId, DateTime ahoraUtc, CancellationToken cancellationToken = default)
        {
            if (solicitud is null) return Task.FromResult<SolicitudDetalleResponse?>(null);
            return Task.FromResult<SolicitudDetalleResponse?>(new(
                solicitud.Id, solicitud.Codigo, solicitud.Titulo,
                solicitud.Descripcion, solicitud.Estado, solicitud.Prioridad,
                new CategoriaResumenResponse(solicitud.CategoriaId, "Incidente"),
                new UsuarioResumenResponse(solicitud.SolicitanteId, "Solicitante"),
                solicitud.AgenteId.HasValue ? new UsuarioResumenResponse(solicitud.AgenteId.Value, "Agente") : null,
                solicitud.FechaCreacion, solicitud.FechaLimiteSla,
                solicitud.FechaResolucion, solicitud.MotivoResolucion,
                solicitud.MotivoCancelacion, false));
        }
    }

    private sealed class RelojFijo(DateTimeOffset ahora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => ahora;
    }
}
