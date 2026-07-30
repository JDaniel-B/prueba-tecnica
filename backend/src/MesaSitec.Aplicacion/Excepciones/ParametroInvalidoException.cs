namespace MesaSitec.Aplicacion.Excepciones;

public sealed class ParametroInvalidoException(string message) : Exception(message);
