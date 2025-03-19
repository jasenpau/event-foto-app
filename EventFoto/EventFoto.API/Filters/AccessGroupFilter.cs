using EventFoto.API.Providers;
using EventFoto.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventFoto.API.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class AccessGroupFilter(UserGroup group) : AuthorizeAttribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        var isAuthenticated = user.Identity?.IsAuthenticated ?? false;
        if (!isAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userGroups = user.FindAll("groups");
        var groupSettingsProvider = context.HttpContext.RequestServices.GetService<IGroupSettingsProvider>();
        if (groupSettingsProvider is null) throw new NullReferenceException("No group settings provider found.");
        var appGroups = groupSettingsProvider.GetGroups();

        var canAccess = group switch
        {
            UserGroup.Photographer => userGroups.Any(c =>
                c.Value == appGroups.Photographers ||
                c.Value == appGroups.EventAdministrators ||
                c.Value == appGroups.SystemAdministrators),
            UserGroup.EventAdmin => userGroups.Any(c =>
                c.Value == appGroups.EventAdministrators ||
                c.Value == appGroups.SystemAdministrators),
            UserGroup.SystemAdmin => userGroups.Any(c =>
                c.Value == appGroups.SystemAdministrators),
            _ => throw new ArgumentException("Invalid user group level requested")
        };
        
        if (!canAccess) context.Result = new UnauthorizedResult();
    }
    
}
