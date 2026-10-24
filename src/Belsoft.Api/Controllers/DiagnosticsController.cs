using Belsoft.Api.Configuration;
using Belsoft.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Belsoft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DiagnosticsController(
    IOptionsSnapshot<BelsoftOptions> options,                                // ①
    IWebHostEnvironment environment) : ControllerBase
{
    // Non-sensitive values only. Restricted to staff in L20.
    [HttpGet("settings")]                                                     // ②
    public ActionResult<SettingsResponse> GetSettings()
    {
        return Ok(new SettingsResponse(environment.EnvironmentName, options.Value.MaxItemsPerOrder)); // ③
    }
}