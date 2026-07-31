using MesaSitec.Aplicacion.Solicitudes.Contratos;

namespace MesaSitec.Aplicacion.Solicitudes.Abstracciones;

public interface IAgenteRepository
{
    Task<bool> EsValidoAsync(
        Guid agenteId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UsuarioResumenResponse>> ListarActivosAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
