using System.ComponentModel.DataAnnotations;

namespace MesaSitec.Aplicacion.Solicitudes.Contratos;

public sealed class TransicionarSolicitudRequest
{
    [Required(ErrorMessage = "La acción es obligatoria.")]
    public string? Accion { get; init; }

    public Guid? AgenteId { get; init; }

    public string? Motivo { get; init; }
}
