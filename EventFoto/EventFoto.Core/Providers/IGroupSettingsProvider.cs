using EventFoto.Data.Models;

namespace EventFoto.Core.Providers;

public interface IGroupSettingsProvider
{
    public AppGroups GetGroups();
    public bool IsValidGroupId(string groupId);
}
