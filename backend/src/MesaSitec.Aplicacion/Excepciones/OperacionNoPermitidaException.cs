namespace MesaSitec.Aplicacion.Excepciones;

public sealed class OperacionNoPermitidaException(string message)
    : Exception(message);
