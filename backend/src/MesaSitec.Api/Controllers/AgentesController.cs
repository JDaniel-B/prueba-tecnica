using MesaSitec.Api.Seguridad;
using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/v1/agentes")]
public sealed class AgentesController(
    IAgenteConsultaService consultaService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UsuarioResumenResponse>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<UsuarioResumenResponse>>> Get(
        CancellationToken cancellationToken)
    {
        var tenantId = User.ObtenerGuidRequerido("tenantId");
        var response = await consultaService.ListarAsync(
            tenantId,
            cancellationToken);

        return Ok(response);
    }
}
