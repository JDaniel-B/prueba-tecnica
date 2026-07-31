using MesaSitec.Aplicacion.Solicitudes.Contratos;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface IAgenteConsultaService
{
    Task<IReadOnlyList<UsuarioResumenResponse>> ListarAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
