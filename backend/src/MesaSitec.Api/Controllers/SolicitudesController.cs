using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MesaSitec.Aplicacion.Excepciones;
using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Dominio.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/solicitudes")]
public sealed class SolicitudesController(
    ISolicitudConsultaService consultaService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginaResponse<SolicitudListadoResponse>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginaResponse<SolicitudListadoResponse>>> Get(
        [FromQuery] ListarSolicitudesRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = ObtenerGuidClaim("tenantId");
        var usuarioId = ObtenerGuidClaim(JwtRegisteredClaimNames.Sub);
        var rol = ObtenerRolClaim();
        var response = await consultaService.ListarAsync(
            request,
            tenantId,
            usuarioId,
            rol,
            cancellationToken);

        return Ok(response);
    }

    private Guid ObtenerGuidClaim(string claim)
    {
        var valor = User.FindFirstValue(claim);
        if (!Guid.TryParse(valor, out var id))
        {
            throw new NoAutenticadoException(
                "El token no contiene los claims requeridos.");
        }

        return id;
    }

    private RolUsuario ObtenerRolClaim()
    {
        var valor = User.FindFirstValue("rol");
        if (!Enum.TryParse<RolUsuario>(valor, out var rol))
        {
            throw new NoAutenticadoException(
                "El token no contiene los claims requeridos.");
        }

        return rol;
    }
}
