using EventFoto.API.Extensions;
using EventFoto.API.Providers;
using EventFoto.Core.Users;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : AppControllerBase
{
    private readonly IUserService _userService;
    private readonly IGroupSettingsProvider _groupSettingsProvider;

    public UserController(
        IUserService userService,
        IGroupSettingsProvider groupSettingsProvider)
    {
        _userService = userService;
        _groupSettingsProvider = groupSettingsProvider;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var userId = RequestUserId();
        if (userId != id) return Unauthorized();
        
        var result = await _userService.GetUser(userId);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto registerDto)
    {
        var userId = RequestUserId();
        var userCreateDetails = new UserCreateDetails
        {
            Email = registerDto.Email,
            Name = registerDto.Name,
        };
        var result = await _userService.CreateUser(userCreateDetails, userId);
        return result.Success
            ? Ok(new UserDto()
            {
                Id = result.Data.Id,
                Email = result.Data.Email,
                Name = result.Data.Name,
            })
            : result.ToErrorResponse();
    }
    
    [HttpGet("groups")]
    public ActionResult<AppGroups> GetGroups()
    {
        var groups = _groupSettingsProvider.GetGroups();
        return Ok(groups);
    }
}
