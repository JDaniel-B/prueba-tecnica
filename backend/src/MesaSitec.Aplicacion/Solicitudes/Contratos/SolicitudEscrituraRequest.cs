using System.ComponentModel.DataAnnotations;
using MesaSitec.Aplicacion.Validacion;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes.Contratos;

public sealed class SolicitudEscrituraRequest
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(
        Solicitud.TituloLongitudMaxima,
        MinimumLength = Solicitud.TituloLongitudMinima,
        ErrorMessage = "El título debe tener entre 5 y 120 caracteres.")]
    public string? Titulo { get; init; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(
        Solicitud.DescripcionLongitudMaxima,
        MinimumLength = Solicitud.DescripcionLongitudMinima,
        ErrorMessage = "La descripción debe tener entre 10 y 4000 caracteres.")]
    public string? Descripcion { get; init; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [GuidNoVacio(ErrorMessage = "La categoría es obligatoria.")]
    public Guid? CategoriaId { get; init; }

    [Required(ErrorMessage = "La prioridad es obligatoria.")]
    [EnumDefinido(ErrorMessage = "La prioridad no contiene un valor permitido.")]
    public PrioridadSolicitud? Prioridad { get; init; }
}
