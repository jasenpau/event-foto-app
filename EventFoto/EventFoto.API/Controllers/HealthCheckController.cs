using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.API.Controllers;

// API route for checking the health status of API service.
// Returns 200 OK, when API is up.

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthCheckController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        return Ok("OK");
    }
}
