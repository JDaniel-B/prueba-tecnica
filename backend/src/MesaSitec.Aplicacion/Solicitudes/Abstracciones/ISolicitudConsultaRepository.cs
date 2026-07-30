using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Aplicacion.Solicitudes.Modelos;

namespace MesaSitec.Aplicacion.Solicitudes.Abstracciones;

public interface ISolicitudConsultaRepository
{
    Task<PaginaResponse<SolicitudListadoResponse>> ListarAsync(
        FiltroSolicitudes filtro,
        CancellationToken cancellationToken = default);
}
