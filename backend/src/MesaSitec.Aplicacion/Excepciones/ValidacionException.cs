namespace MesaSitec.Aplicacion.Excepciones;

public sealed class ValidacionException(
    IReadOnlyDictionary<string, string[]> errores)
    : Exception("Uno o más campos no son válidos.")
{
    public IReadOnlyDictionary<string, string[]> Errores { get; } = errores;
}
