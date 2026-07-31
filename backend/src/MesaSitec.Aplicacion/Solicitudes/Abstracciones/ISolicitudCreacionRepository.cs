using MesaSitec.Aplicacion.Solicitudes.Modelos;
using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Solicitudes.Abstracciones;

public interface ISolicitudCreacionRepository
{
    Task<CategoriaParaSolicitud?> BuscarCategoriaActivaAsync(
        Guid categoriaId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<int> ObtenerSiguienteCorrelativoAsync(
        Guid tenantId,
        int anio,
        CancellationToken cancellationToken = default);

    Task GuardarAsync(
        Solicitud solicitud,
        CancellationToken cancellationToken = default);
}
