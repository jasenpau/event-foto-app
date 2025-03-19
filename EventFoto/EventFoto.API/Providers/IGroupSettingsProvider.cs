using EventFoto.Data.Models;

namespace EventFoto.API.Providers;

public interface IGroupSettingsProvider
{
    public AppGroups GetGroups();
}
