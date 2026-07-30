using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Autenticacion.Modelos;

public sealed record UsuarioAutenticacion(
    Usuario Usuario,
    string TenantNombre,
    bool TenantActivo);
