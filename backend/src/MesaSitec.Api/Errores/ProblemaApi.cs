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
        CancellationToken cancellationToken = default,
        IReadOnlyDictionary<string, string[]>? errores = null)
    {
        context.Response.StatusCode = status;

        if (errores is not null)
        {
            var problemaConErrores = new
            {
                type = $"https://mesasitec.local/errores/{tipo}",
                title,
                status,
                detail,
                codigo,
                errores
            };

            return context.Response.WriteAsJsonAsync(
                problemaConErrores,
                options: null,
                contentType: "application/problem+json",
                cancellationToken);
        }

        return context.Response.WriteAsJsonAsync(
            new
            {
                type = $"https://mesasitec.local/errores/{tipo}",
                title,
                status,
                detail,
                codigo
            },
            options: null,
            contentType: "application/problem+json",
            cancellationToken);
    }
}
