using MesaSitec.Aplicacion.Excepciones;
using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public sealed class SolicitudActualizacionService(
    ISolicitudEscrituraRepository escrituraRepository,
    ISolicitudConsultaRepository consultaRepository,
    TimeProvider timeProvider) : ISolicitudActualizacionService
{
    public async Task<SolicitudDetalleResponse> ActualizarAsync(
        Guid id,
        SolicitudEscrituraRequest request,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        CancellationToken cancellationToken = default)
    {
        ValidadorSolicitudEscritura.Validar(request);

        var solicitud = await escrituraRepository.BuscarAsync(
            id,
            tenantId,
            cancellationToken);
        if (solicitud is null)
        {
            throw new RecursoNoEncontradoException(
                "La solicitud no existe.");
        }

        if (rol == RolUsuario.Solicitante
            && (solicitud.SolicitanteId != usuarioId
                || solicitud.Estado != EstadoSolicitud.Nueva))
        {
            throw new OperacionNoPermitidaException(
                "El usuario no puede editar esta solicitud.");
        }

        var categoria = await escrituraRepository.BuscarCategoriaActivaAsync(
            request.CategoriaId!.Value,
            tenantId,
            cancellationToken);
        if (categoria is null)
        {
            throw ValidadorSolicitudEscritura.CrearError(
                "categoriaId",
                "La categoría no existe, está inactiva o pertenece a otra organización.");
        }

        solicitud.Actualizar(
            request.Titulo!,
            request.Descripcion!,
            categoria.Id,
            request.Prioridad!.Value,
            categoria.SlaHoras);
        await escrituraRepository.GuardarCambiosAsync(cancellationToken);

        return await consultaRepository.ObtenerDetalleAsync(
                solicitud.Id,
                tenantId,
                timeProvider.GetUtcNow().UtcDateTime,
                cancellationToken)
            ?? throw new InvalidOperationException(
                "No fue posible recuperar la solicitud actualizada.");
    }
}
