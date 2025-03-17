using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EventFoto.API.Extensions;
using EventFoto.Core.Authentication;
using EventFoto.Data.DTOs;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;

namespace EventFoto.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (result.Success)
        {
            return new LoginResponseDto
            {
                Token = result.Data
            };
        }

        if (result.Error is
            AppError.UserNotFound or
            AppError.NoCredentials or
            AppError.InvalidCredentials)
        {
            return Problem(
                title: AppErrorMessage.InvalidCredentials,
                statusCode: StatusCodes.Status401Unauthorized
                );
        }

        return Problem(
            title: AppErrorMessage.InternalError,
            statusCode: StatusCodes.Status500InternalServerError
        );
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        var userCreateDetails = new UserCreateDetails
        {
            Name = request.Name,
            Email = request.Email,
        };
        
        var result = await _authService.RegisterPasswordAsync(userCreateDetails, request.Password);

        if (result.Success)
        {
            return new RegisterResponseDto
            {
                Token = result.Data
            };
        }
        
        return Problem(
            title: AppErrorMessage.InternalError,
            statusCode: StatusCodes.Status500InternalServerError
        );
    }

    [HttpGet("entra-status")]
    public async Task<ActionResult<EntraStatusResponseDto>> EntraStatus()
    {
        var userIdString = HttpContext.User.FindFirst(AppClaims.UserId)?.Value;
        var hasId = int.TryParse(userIdString, out var userId);
        if (hasId)
        {
            // return userId;
        }
        
        var objectIdString = HttpContext.User.FindFirst(ClaimConstants.ObjectId)?.Value;
        var hasObjectId = Guid.TryParse(objectIdString, out var objectId);

        return new EntraStatusResponseDto
        {
            EntraStatus = "not-found"
        };
    }
}
