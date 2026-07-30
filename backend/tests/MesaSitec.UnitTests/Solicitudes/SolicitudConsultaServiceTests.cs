using MesaSitec.Aplicacion.Excepciones;
using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Aplicacion.Solicitudes.Modelos;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.UnitTests.Solicitudes;

public sealed class SolicitudConsultaServiceTests
{
    private static readonly DateTimeOffset Ahora =
        new(2026, 1, 15, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Listar_SolicitanteSoloConsultaSusPropiasSolicitudes()
    {
        var repositorio = new RepositorioCaptura();
        var servicio = new SolicitudConsultaService(
            repositorio,
            new RelojFijo(Ahora));
        var tenantId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var request = new ListarSolicitudesRequest
        {
            Q = "  portal  ",
            Sort = "codigo"
        };

        await servicio.ListarAsync(
            request,
            tenantId,
            usuarioId,
            RolUsuario.Solicitante);

        Assert.NotNull(repositorio.UltimoFiltro);
        Assert.Equal(tenantId, repositorio.UltimoFiltro.TenantId);
        Assert.Equal(usuarioId, repositorio.UltimoFiltro.SolicitanteId);
        Assert.Equal("portal", repositorio.UltimoFiltro.Busqueda);
        Assert.Equal("codigo", repositorio.UltimoFiltro.Orden);
        Assert.Equal(Ahora.UtcDateTime, repositorio.UltimoFiltro.AhoraUtc);
    }

    [Theory]
    [InlineData(RolUsuario.Admin)]
    [InlineData(RolUsuario.Agente)]
    public async Task Listar_AdminYAgenteConsultanTodoSuTenant(RolUsuario rol)
    {
        var repositorio = new RepositorioCaptura();
        var servicio = new SolicitudConsultaService(
            repositorio,
            new RelojFijo(Ahora));

        await servicio.ListarAsync(
            new ListarSolicitudesRequest(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            rol);

        Assert.NotNull(repositorio.UltimoFiltro);
        Assert.Null(repositorio.UltimoFiltro.SolicitanteId);
    }

    [Theory]
    [InlineData(0, 20, "-fechaCreacion")]
    [InlineData(1, 0, "-fechaCreacion")]
    [InlineData(1, 101, "-fechaCreacion")]
    [InlineData(1, 20, "titulo")]
    public async Task Listar_RechazaParametrosFueraDeContrato(
        int page,
        int pageSize,
        string sort)
    {
        var servicio = new SolicitudConsultaService(
            new RepositorioCaptura(),
            new RelojFijo(Ahora));
        var request = new ListarSolicitudesRequest
        {
            Page = page,
            PageSize = pageSize,
            Sort = sort
        };

        await Assert.ThrowsAsync<ParametroInvalidoException>(
            () => servicio.ListarAsync(
                request,
                Guid.NewGuid(),
                Guid.NewGuid(),
                RolUsuario.Admin));
    }

    [Fact]
    public async Task Listar_RechazaValoresNumericosFueraDeLosEnums()
    {
        var servicio = new SolicitudConsultaService(
            new RepositorioCaptura(),
            new RelojFijo(Ahora));
        var request = new ListarSolicitudesRequest
        {
            Estado = (EstadoSolicitud)99
        };

        await Assert.ThrowsAsync<ParametroInvalidoException>(
            () => servicio.ListarAsync(
                request,
                Guid.NewGuid(),
                Guid.NewGuid(),
                RolUsuario.Admin));
    }

    private sealed class RepositorioCaptura : ISolicitudConsultaRepository
    {
        public FiltroSolicitudes? UltimoFiltro { get; private set; }

        public Task<PaginaResponse<SolicitudListadoResponse>> ListarAsync(
            FiltroSolicitudes filtro,
            CancellationToken cancellationToken = default)
        {
            UltimoFiltro = filtro;
            return Task.FromResult(
                new PaginaResponse<SolicitudListadoResponse>(
                    [],
                    filtro.Page,
                    filtro.PageSize,
                    0,
                    0));
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
