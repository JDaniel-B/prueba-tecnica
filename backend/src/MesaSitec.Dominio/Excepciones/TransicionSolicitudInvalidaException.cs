using MesaSitec.Dominio.Enums;

namespace MesaSitec.Dominio.Excepciones;

public sealed class TransicionSolicitudInvalidaException(
    EstadoSolicitud estado,
    string accion)
    : Exception(
        $"La acción '{accion}' no está permitida desde el estado '{estado}'.")
{
}
