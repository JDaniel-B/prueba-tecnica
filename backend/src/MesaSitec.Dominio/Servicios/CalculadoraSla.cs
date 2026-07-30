using MesaSitec.Dominio.Enums;

namespace MesaSitec.Dominio.Servicios;

public static class CalculadoraSla
{
    public static DateTime Calcular(
        DateTime fechaCreacion,
        int slaHoras,
        PrioridadSolicitud prioridad)
    {
        if (fechaCreacion.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "La fecha de creación debe estar expresada en UTC.",
                nameof(fechaCreacion));
        }

        if (slaHoras <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(slaHoras),
                "Las horas de SLA deben ser mayores que cero.");
        }

        var factor = prioridad switch
        {
            PrioridadSolicitud.Critica => 0.5,
            PrioridadSolicitud.Alta => 0.75,
            PrioridadSolicitud.Media => 1.0,
            PrioridadSolicitud.Baja => 2.0,
            _ => throw new ArgumentOutOfRangeException(
                nameof(prioridad),
                prioridad,
                "La prioridad no es válida.")
        };

        return fechaCreacion.AddHours(slaHoras * factor);
    }
}
