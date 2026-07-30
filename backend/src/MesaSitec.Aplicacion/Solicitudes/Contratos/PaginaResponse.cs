namespace MesaSitec.Aplicacion.Solicitudes.Contratos;

public sealed record PaginaResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total,
    int TotalPaginas);
