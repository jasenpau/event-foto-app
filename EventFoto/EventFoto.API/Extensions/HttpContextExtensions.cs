using System.Security.Authentication;
using EventFoto.Data.Enums;

namespace EventFoto.API.Extensions;

public static class HttpContextExtensions
{
    public static int RequestUserId(this HttpContext httpContext)
    {
        var userIdString = httpContext.User.FindFirst(AppClaims.UserId)?.Value;
        var hasId = int.TryParse(userIdString, out var userId);
        
        if (!hasId)
        {
            throw new AuthenticationException("Invalid user id");
        }

        return userId;
    }
}
