using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Aplicacion.Solicitudes.Contratos;

namespace MesaSitec.Aplicacion.Solicitudes;

public sealed class AgenteConsultaService(
    IAgenteRepository repository) : IAgenteConsultaService
{
    public Task<IReadOnlyList<UsuarioResumenResponse>> ListarAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return repository.ListarActivosAsync(tenantId, cancellationToken);
    }
}
