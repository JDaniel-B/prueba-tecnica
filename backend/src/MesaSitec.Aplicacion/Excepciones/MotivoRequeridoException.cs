namespace MesaSitec.Aplicacion.Excepciones;

public sealed class MotivoRequeridoException(string message) : Exception(message)
{
}
