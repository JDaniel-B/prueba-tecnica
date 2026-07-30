namespace MesaSitec.Aplicacion.Excepciones;

public sealed class NoAutenticadoException(string message) : Exception(message);
