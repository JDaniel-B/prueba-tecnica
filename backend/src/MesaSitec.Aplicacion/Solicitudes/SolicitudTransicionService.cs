using MesaSitec.Aplicacion.Excepciones;
using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public sealed class SolicitudTransicionService(
    ISolicitudEscrituraRepository escrituraRepository,
    IAgenteRepository agenteRepository,
    ISolicitudConsultaRepository consultaRepository,
    TimeProvider timeProvider) : ISolicitudTransicionService
{
    public async Task<SolicitudDetalleResponse> TransicionarAsync(
        Guid id,
        TransicionarSolicitudRequest request,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        CancellationToken cancellationToken = default)
    {
        var solicitud = await escrituraRepository.BuscarAsync(
            id,
            tenantId,
            cancellationToken)
            ?? throw new RecursoNoEncontradoException("La solicitud no existe.");
        var accion = request.Accion?.Trim().ToLowerInvariant() ?? string.Empty;

        ValidarPermiso(accion, rol, solicitud.SolicitanteId == usuarioId);

        switch (accion)
        {
            case "asignar":
                await AsignarAsync(
                    solicitud,
                    request.AgenteId,
                    tenantId,
                    cancellationToken);
                break;
            case "iniciar":
                solicitud.Iniciar();
                break;
            case "resolver":
                ValidarMotivo(request.Motivo, 20, "resolver");
                solicitud.Resolver(
                    request.Motivo!,
                    timeProvider.GetUtcNow().UtcDateTime);
                break;
            case "cerrar":
                solicitud.Cerrar();
                break;
            case "reabrir":
                solicitud.Reabrir();
                break;
            case "cancelar":
                ValidarMotivo(request.Motivo, 10, "cancelar");
                solicitud.Cancelar(request.Motivo!);
                break;
            default:
                throw new Dominio.Excepciones.TransicionSolicitudInvalidaException(
                    solicitud.Estado,
                    accion);
        }

        await escrituraRepository.GuardarCambiosAsync(cancellationToken);

        return await consultaRepository.ObtenerDetalleAsync(
                solicitud.Id,
                tenantId,
                timeProvider.GetUtcNow().UtcDateTime,
                cancellationToken)
            ?? throw new InvalidOperationException(
                "No fue posible recuperar la solicitud actualizada.");
    }

    private async Task AsignarAsync(
        Dominio.Entidades.Solicitud solicitud,
        Guid? agenteId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!agenteId.HasValue
            || agenteId == Guid.Empty
            || !await agenteRepository.EsValidoAsync(
                agenteId.Value,
                tenantId,
                cancellationToken))
        {
            throw new AgenteInvalidoException(
                "El agente no existe, está inactivo o no pertenece a la organización.");
        }

        solicitud.Asignar(agenteId.Value);
    }

    private static void ValidarPermiso(
        string accion,
        RolUsuario rol,
        bool esPropia)
    {
        var permitido = accion switch
        {
            "asignar" or "iniciar" or "resolver" or "reabrir" =>
                rol is RolUsuario.Admin or RolUsuario.Agente,
            "cerrar" => rol is RolUsuario.Admin or RolUsuario.Agente
                || rol == RolUsuario.Solicitante && esPropia,
            "cancelar" => rol == RolUsuario.Admin,
            _ => true
        };

        if (!permitido)
        {
            throw new OperacionNoPermitidaException(
                "El usuario no puede ejecutar esta acción.");
        }
    }

    private static void ValidarMotivo(
        string? motivo,
        int longitudMinima,
        string accion)
    {
        if (string.IsNullOrWhiteSpace(motivo)
            || motivo.Trim().Length < longitudMinima)
        {
            throw new MotivoRequeridoException(
                $"La acción '{accion}' requiere un motivo de al menos {longitudMinima} caracteres.");
        }
    }
}
