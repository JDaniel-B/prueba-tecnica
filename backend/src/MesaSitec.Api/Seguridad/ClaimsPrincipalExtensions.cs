using System.Security.Claims;
using MesaSitec.Aplicacion.Excepciones;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.Api.Seguridad;

public static class ClaimsPrincipalExtensions
{
    public static Guid ObtenerGuidRequerido(
        this ClaimsPrincipal principal,
        string claim)
    {
        var valor = principal.FindFirstValue(claim);
        if (!Guid.TryParse(valor, out var id) || id == Guid.Empty)
        {
            throw CrearExcepcionClaims();
        }

        return id;
    }

    public static RolUsuario ObtenerRolRequerido(
        this ClaimsPrincipal principal)
    {
        var valor = principal.FindFirstValue("rol");
        if (!Enum.TryParse<RolUsuario>(valor, out var rol)
            || !Enum.IsDefined(rol))
        {
            throw CrearExcepcionClaims();
        }

        return rol;
    }

    private static NoAutenticadoException CrearExcepcionClaims()
    {
        return new NoAutenticadoException(
            "El token no contiene los claims requeridos.");
    }
}
