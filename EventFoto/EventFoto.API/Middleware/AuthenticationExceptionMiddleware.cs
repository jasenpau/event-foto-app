using System.Net;
using EventFoto.API.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.API.Middleware;

public class AuthenticationExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidUserIdException ex)
        {
            var unauthorizedProblemDetails = new ProblemDetails
            {
                Detail = ex.Message,
                Status = (int)HttpStatusCode.Unauthorized
            };
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(unauthorizedProblemDetails);
        }
    }
}

public static class AuthenticationExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthenticationExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationExceptionMiddleware>();
    }
}
