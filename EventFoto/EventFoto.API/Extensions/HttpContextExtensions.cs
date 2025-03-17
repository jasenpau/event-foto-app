using System.Security.Authentication;
using EventFoto.Data.Enums;
using Microsoft.Identity.Web;

namespace EventFoto.API.Extensions;

public static class HttpContextExtensions
{
    public static int RequestUserId(this HttpContext httpContext)
    {
        var userIdString = httpContext.User.FindFirst(AppClaims.UserId)?.Value;
        var hasId = int.TryParse(userIdString, out var userId);
        if (hasId)
        {
            return userId;
        }
        
        var objectId = httpContext.User.FindFirst(ClaimConstants.ObjectId)?.Value;

        throw new AuthenticationException("Invalid user id");
    }
}
