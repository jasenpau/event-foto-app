using EventFoto.API.Extensions;
using EventFoto.Core.Providers;
using EventFoto.Core.Users;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : AppControllerBase
{
    private readonly IUserService _userService;
    private readonly IGroupSettingsProvider _groupProvider;

    public UserController(
        IUserService userService,
        IGroupSettingsProvider groupProvider)
    {
        _userService = userService;
        _groupProvider = groupProvider;
    }

    [HttpGet("current")]
    public async Task<ActionResult<UserDto>> GetCurrentUserAsync()
    {
        var userId = RequestUserId();
        var result = await _userService.GetUserAsync(userId);

        return result.Success ? Ok(UserDto.FromModel(result.Data)) : result.ToErrorResponse();
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> InviteRegister([FromBody] RegisterDto registerDto)
    {
        var userId = RequestUserId();
        var result = await _userService.InviteRegisterAsync(registerDto, userId);
        return result.Success
            ? Ok(UserDto.FromModel(result.Data))
            : result.ToErrorResponse();
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedData<string, UserListDto>>> SearchUsers([FromQuery] UserSearchParams searchParams)
    {
        var searchResult = await _userService.SearchUsersAsync(searchParams);
        if (!searchResult.Success)
        {
            return searchResult.ToErrorResponse();
        }

        var result = searchResult.Data.ToDto(UserListDto.FromUser);
        return Ok(result);
    }

    [HttpPost("invite")]
    public async Task<ActionResult<Guid>> InviteUser([FromBody] UserInviteRequestDto inviteDto)
    {
        if (!string.IsNullOrEmpty(inviteDto.GroupAssignment) &&
            !_groupProvider.IsValidGroupId(inviteDto.GroupAssignment))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Group assignment is invalid."
            });
        }

        var result = await _userService.InviteUserAsync(inviteDto);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }

    [HttpPost("validate-invite")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> ValidateInvite([FromBody] InviteValidateRequestDto dto)
    {
        var result = await _userService.ValidateInviteKey(dto.InviteKey);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }
}
