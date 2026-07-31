using MesaSitec.Aplicacion.Solicitudes.Modelos;
using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Solicitudes.Abstracciones;

public interface ISolicitudEscrituraRepository
{
    Task<CategoriaParaSolicitud?> BuscarCategoriaActivaAsync(
        Guid categoriaId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<Solicitud?> BuscarAsync(
        Guid id,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<int> ObtenerSiguienteCorrelativoAsync(
        Guid tenantId,
        int anio,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Solicitud solicitud,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}
