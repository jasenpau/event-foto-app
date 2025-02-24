using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EventFoto.API.Services;
using EventFoto.Data.DTOs;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EventFoto.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }
    
    [HttpPost("login")]
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
}
