using System.Security.Authentication;
using EventFoto.API.Providers;
using EventFoto.Data.Enums;
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

    protected UserGroup? RequestHighestUserGroup()
    {
        var user = HttpContext.User;
        var isAuthenticated = user.Identity?.IsAuthenticated ?? false;
        if (!isAuthenticated)
        {
            return null;
        }

        var groupSettingsProvider = HttpContext.RequestServices.GetService<IGroupSettingsProvider>();
        if (groupSettingsProvider is null) throw new NullReferenceException("No group settings provider found.");
        var appGroups = groupSettingsProvider.GetGroups();

        var userGroups = user.FindAll("groups").ToList();
        if (userGroups.Any(x => x.Value == appGroups.SystemAdministrators)) return UserGroup.EventAdmin;
        if (userGroups.Any(x => x.Value == appGroups.EventAdministrators)) return UserGroup.EventAdmin;
        if (userGroups.Any(x => x.Value == appGroups.Photographers)) return UserGroup.Photographer;

        return null;
    }
}
