using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [Authorize]
    [HttpGet("tenant")]
    public IActionResult GetTenant()
    {
        var tenantId = User.FindFirst("tenantId")?.Value;

        return Ok(new
        {
            tenantId
        });
    }
}