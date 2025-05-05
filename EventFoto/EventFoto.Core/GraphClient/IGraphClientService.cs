using Microsoft.Graph.Models;

namespace EventFoto.Core.GraphClient;

public interface IGraphClientService
{
    public Task<Invitation?> InviteUserAsync(string email, string name, Guid invitationKey);
    public Task AssignUserGroup(string userId, string groupId);
}
