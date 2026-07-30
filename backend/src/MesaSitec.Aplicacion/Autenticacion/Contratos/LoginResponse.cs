namespace MesaSitec.Aplicacion.Autenticacion.Contratos;

public sealed record LoginResponse(
    string AccessToken,
    int ExpiraEn,
    UsuarioResponse Usuario);
