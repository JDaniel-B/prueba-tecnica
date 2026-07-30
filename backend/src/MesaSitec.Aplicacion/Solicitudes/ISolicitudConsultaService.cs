using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudConsultaService
{
    Task<PaginaResponse<SolicitudListadoResponse>> ListarAsync(
        ListarSolicitudesRequest request,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        CancellationToken cancellationToken = default);
}
