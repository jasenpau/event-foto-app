using EventFoto.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    [HttpGet("count")]
    [AllowAnonymous]
    public IActionResult Count()
    {
        return Ok("14");
    }

    [HttpGet("current")]
    [Authorize]
    public IActionResult GetUser()
    {
        return Ok("AAA");
    }
}
