namespace MesaSitec.Api.Errores;

public static class ProblemaApi
{
    public static Task EscribirAsync(
        HttpContext context,
        int status,
        string codigo,
        string title,
        string detail,
        string tipo,
        CancellationToken cancellationToken = default)
    {
        context.Response.StatusCode = status;

        var problema = new
        {
            type = $"https://mesasitec.local/errores/{tipo}",
            title,
            status,
            detail,
            codigo
        };

        return context.Response.WriteAsJsonAsync(
            problema,
            options: null,
            contentType: "application/problem+json",
            cancellationToken);
    }
}
