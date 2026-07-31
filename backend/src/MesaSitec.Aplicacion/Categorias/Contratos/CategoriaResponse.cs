namespace MesaSitec.Aplicacion.Categorias.Contratos;

public sealed record CategoriaResponse(
    Guid Id,
    string Nombre,
    int SlaHoras);
