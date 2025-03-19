using System.Security.Authentication;
using EventFoto.API.Providers;
using EventFoto.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EventFoto.API.Controllers;

public class AppControllerBase : ControllerBase
{
    protected Guid RequestUserId()
    {
        var objectIdString = HttpContext.User.FindFirst(ClaimConstants.ObjectId)?.Value;
        var hasObjectId = Guid.TryParse(objectIdString, out var objectId);
        if (hasObjectId)
        {
            return objectId;
        }

        throw new AuthenticationException("Invalid user id");
    }
}
