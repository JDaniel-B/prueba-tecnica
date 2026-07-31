using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudActualizacionService
{
    Task<SolicitudDetalleResponse> ActualizarAsync(
        Guid id,
        SolicitudEscrituraRequest request,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        CancellationToken cancellationToken = default);
}
