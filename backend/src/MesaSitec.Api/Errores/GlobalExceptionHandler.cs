using MesaSitec.Aplicacion.Excepciones;
using Microsoft.AspNetCore.Diagnostics;

namespace MesaSitec.Api.Errores;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problema = exception switch
        {
            NoAutenticadoException => new DatosProblema(
                StatusCodes.Status401Unauthorized,
                "NO_AUTENTICADO",
                "No autenticado",
                exception.Message,
                "no-autenticado"),
            RecursoNoEncontradoException => new DatosProblema(
                StatusCodes.Status404NotFound,
                "RECURSO_NO_ENCONTRADO",
                "Recurso no encontrado",
                exception.Message,
                "recurso-no-encontrado"),
            _ => new DatosProblema(
                StatusCodes.Status500InternalServerError,
                "ERROR_INTERNO",
                "Error interno",
                "Ocurrió un error inesperado.",
                "error-interno")
        };

        if (problema.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Error no controlado al procesar la solicitud.");
        }

        await ProblemaApi.EscribirAsync(
            httpContext,
            problema.Status,
            problema.Codigo,
            problema.Title,
            problema.Detail,
            problema.Tipo,
            cancellationToken);

        return true;
    }

    private sealed record DatosProblema(
        int Status,
        string Codigo,
        string Title,
        string Detail,
        string Tipo);
}
