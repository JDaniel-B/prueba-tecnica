using MesaSitec.Aplicacion.Excepciones;
using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Aplicacion.Solicitudes.Modelos;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public sealed class SolicitudConsultaService(
    ISolicitudConsultaRepository repositorio,
    TimeProvider timeProvider) : ISolicitudConsultaService
{
    private static readonly HashSet<string> OrdenesPermitidos =
        new(StringComparer.Ordinal)
        {
            "fechaCreacion",
            "-fechaCreacion",
            "prioridad",
            "-prioridad",
            "codigo"
        };

    public Task<PaginaResponse<SolicitudListadoResponse>> ListarAsync(
        ListarSolicitudesRequest request,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        CancellationToken cancellationToken = default)
    {
        Validar(request);

        var orden = string.IsNullOrWhiteSpace(request.Sort)
            ? "-fechaCreacion"
            : request.Sort.Trim();
        var busqueda = string.IsNullOrWhiteSpace(request.Q)
            ? null
            : request.Q.Trim();
        Guid? solicitanteId = rol == RolUsuario.Solicitante
            ? usuarioId
            : null;
        var filtro = new FiltroSolicitudes(
            tenantId,
            solicitanteId,
            request.Estado,
            request.Prioridad,
            request.CategoriaId,
            request.AgenteId,
            busqueda,
            request.Vencidas,
            timeProvider.GetUtcNow().UtcDateTime,
            request.Page,
            request.PageSize,
            orden);

        return repositorio.ListarAsync(filtro, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> ObtenerDetalleAsync(
        Guid id,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        CancellationToken cancellationToken = default)
    {
        var solicitud = await repositorio.ObtenerDetalleAsync(
            id,
            tenantId,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);
        if (solicitud is null)
        {
            throw new RecursoNoEncontradoException(
                "La solicitud no existe.");
        }

        if (rol == RolUsuario.Solicitante
            && solicitud.Solicitante.Id != usuarioId)
        {
            throw new OperacionNoPermitidaException(
                "El usuario no puede consultar esta solicitud.");
        }

        return solicitud;
    }

    private static void Validar(ListarSolicitudesRequest request)
    {
        if (request.Estado.HasValue
            && !Enum.IsDefined(request.Estado.Value))
        {
            throw new ParametroInvalidoException(
                "El parámetro 'estado' no contiene un valor permitido.");
        }

        if (request.Prioridad.HasValue
            && !Enum.IsDefined(request.Prioridad.Value))
        {
            throw new ParametroInvalidoException(
                "El parámetro 'prioridad' no contiene un valor permitido.");
        }

        if (request.Page < 1)
        {
            throw new ParametroInvalidoException(
                "El parámetro 'page' debe ser mayor o igual a 1.");
        }

        if (request.PageSize is < 1 or > 100)
        {
            throw new ParametroInvalidoException(
                "El parámetro 'pageSize' debe estar entre 1 y 100.");
        }

        var orden = string.IsNullOrWhiteSpace(request.Sort)
            ? "-fechaCreacion"
            : request.Sort.Trim();
        if (!OrdenesPermitidos.Contains(orden))
        {
            throw new ParametroInvalidoException(
                "El parámetro 'sort' no contiene un valor permitido.");
        }
    }
}
