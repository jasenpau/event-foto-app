using System.Security.Authentication;
using EventFoto.Core.Providers;
using EventFoto.Data.Enums;
using Microsoft.Identity.Web;

namespace EventFoto.API.Controllers;

public class AppControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
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

    protected UserGroup? RequestHighestUserGroup()
    {
        var user = HttpContext.User;
        var isAuthenticated = user.Identity?.IsAuthenticated ?? false;
        if (!isAuthenticated)
        {
            return null;
        }

        var groupSettingsProvider = HttpContext.RequestServices.GetRequiredService<IGroupSettingsProvider>();
        var appGroups = groupSettingsProvider.GetGroups();

        var userGroups = user.FindAll("groups").ToList();
        if (userGroups.Any(x => x.Value == appGroups.SystemAdministrators)) return UserGroup.EventAdmin;
        if (userGroups.Any(x => x.Value == appGroups.EventAdministrators)) return UserGroup.EventAdmin;
        if (userGroups.Any(x => x.Value == appGroups.Photographers)) return UserGroup.Photographer;

        return null;
    }
}
