using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes.Contratos;

public sealed record SolicitudListadoResponse(
    Guid Id,
    string Codigo,
    string Titulo,
    EstadoSolicitud Estado,
    PrioridadSolicitud Prioridad,
    CategoriaResumenResponse Categoria,
    UsuarioResumenResponse? Agente,
    DateTime FechaCreacion,
    DateTime FechaLimiteSla,
    bool Vencida);
