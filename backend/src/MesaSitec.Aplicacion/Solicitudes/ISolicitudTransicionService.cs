using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudTransicionService
{
    Task<SolicitudDetalleResponse> TransicionarAsync(
        Guid id,
        TransicionarSolicitudRequest request,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        CancellationToken cancellationToken = default);
}
