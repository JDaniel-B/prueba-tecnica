using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes.Contratos;

public sealed record SolicitudDetalleResponse(
    Guid Id,
    string Codigo,
    string Titulo,
    string Descripcion,
    EstadoSolicitud Estado,
    PrioridadSolicitud Prioridad,
    CategoriaResumenResponse Categoria,
    UsuarioResumenResponse Solicitante,
    UsuarioResumenResponse? Agente,
    DateTime FechaCreacion,
    DateTime FechaLimiteSla,
    DateTime? FechaResolucion,
    string? MotivoResolucion,
    string? MotivoCancelacion,
    bool Vencida);
