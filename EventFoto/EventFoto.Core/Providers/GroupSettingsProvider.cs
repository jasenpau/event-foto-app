using EventFoto.Data.Models;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Core.Providers;

public class GroupSettingsProvider : IGroupSettingsProvider
{
    private readonly AppGroups _groups;
    
    public GroupSettingsProvider(IConfiguration configuration)
    {
        var systemAdminGroupIdString = configuration["Groups:SystemAdminGroupId"];
        var eventAdminGroupIdString = configuration["Groups:EventAdminGroupId"];
        var photographerGroupIdString = configuration["Groups:PhotographerGroupId"];

        if (systemAdminGroupIdString is null || eventAdminGroupIdString is null || photographerGroupIdString is null)
        {
            throw new NullReferenceException("Group IDs cannot be null. Please check appsettings.");
        }

        _groups = new AppGroups
        {
            SystemAdministrators = systemAdminGroupIdString,
            EventAdministrators = eventAdminGroupIdString,
            Photographers = photographerGroupIdString
        };
    }
    
    public AppGroups GetGroups() => _groups;
}
