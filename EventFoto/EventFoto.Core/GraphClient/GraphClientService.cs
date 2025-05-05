using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace EventFoto.Core.GraphClient;

public class GraphClientService : IGraphClientService
{
    private readonly IConfiguration _configuration;
    private readonly GraphServiceClient _graphClient;

    public Task<Invitation?> InviteUserAsync(string email, string name, Guid invitationKey)
    {
        var invitation = new Invitation
        {
            InvitedUserEmailAddress = email,
            InvitedUserDisplayName = name,
            SendInvitationMessage = true,
            InviteRedirectUrl = $"{_configuration["PublicAppUrl"]}/invite/{invitationKey}"
        };

        return _graphClient.Invitations.PostAsync(invitation);
    }

    public async Task AssignUserGroup(string userId, string groupId)
    {
        var requestBody = new ReferenceCreate
        {
            OdataId = $"https://graph.microsoft.com/v1.0/directoryObjects/{userId}",
        };
        await _graphClient.Groups[groupId].Members.Ref.PostAsync(requestBody);
    }
}
