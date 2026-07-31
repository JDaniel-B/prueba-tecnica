using System.IdentityModel.Tokens.Jwt;
using MesaSitec.Api.Seguridad;
using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
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
        var tenantId = User.ObtenerGuidRequerido("tenantId");
        var usuarioId = User.ObtenerGuidRequerido(JwtRegisteredClaimNames.Sub);
        var rol = User.ObtenerRolRequerido();
        var response = await consultaService.ListarAsync(
            request,
            tenantId,
            usuarioId,
            rol,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<SolicitudDetalleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantId = User.ObtenerGuidRequerido("tenantId");
        var usuarioId = User.ObtenerGuidRequerido(JwtRegisteredClaimNames.Sub);
        var rol = User.ObtenerRolRequerido();
        var response = await consultaService.ObtenerDetalleAsync(
            id,
            tenantId,
            usuarioId,
            rol,
            cancellationToken);

        return Ok(response);
    }
}
