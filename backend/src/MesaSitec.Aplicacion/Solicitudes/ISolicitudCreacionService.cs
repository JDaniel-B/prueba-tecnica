using MesaSitec.Aplicacion.Solicitudes.Contratos;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudCreacionService
{
    Task<SolicitudDetalleResponse> CrearAsync(
        CrearSolicitudRequest request,
        Guid tenantId,
        Guid usuarioId,
        CancellationToken cancellationToken = default);
}
