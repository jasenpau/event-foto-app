using System.Net;
using EventFoto.Core.GraphClient;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using User = EventFoto.Data.Models.User;

namespace EventFoto.Core.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IGraphClientService _graphClient;

    public UserService(IUserRepository userRepository,
        IGraphClientService graphClient)
    {
        _userRepository = userRepository;
        _graphClient = graphClient;
    }
    
    public async Task<ServiceResult<User>> GetUserAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return ServiceResult<User>.Fail("User not found", HttpStatusCode.NotFound);
        return ServiceResult<User>.Ok(user);
    }

    public async Task<ServiceResult<User>> InviteRegisterAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);

        user.IsActive = true;
        user = await _userRepository.UpdateUserAsync(user);

        return ServiceResult<User>.Ok(user);
    }

    public async Task<ServiceResult<PagedData<string, User>>> SearchUsersAsync(UserSearchParams searchParams)
    {
        var result = await _userRepository.SearchUsersAsync(searchParams);
        return result is not null
            ? ServiceResult<PagedData<string, User>>.Ok(result)
            : ServiceResult<PagedData<string, User>>.Fail("Query failed", HttpStatusCode.InternalServerError);
    }

    public async Task<ServiceResult<Guid>> InviteUserAsync(UserInviteRequestDto inviteRequestDto)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetUserByEmailAsync(inviteRequestDto.Email);
        if (existingUser != null)
        {
            return ServiceResult<Guid>.Fail("User with this email already exists", "exists", HttpStatusCode.Conflict);
        }

        var invitationKey = Guid.NewGuid();

        var inviteGraphResult = await _graphClient.InviteUserAsync(inviteRequestDto.Email, inviteRequestDto.Name, invitationKey);
        if (inviteGraphResult?.InvitedUser?.Id == null)
        {
            return ServiceResult<Guid>.Fail("Failed to create invitation in Entra", HttpStatusCode.InternalServerError);
        }

        if (!string.IsNullOrEmpty(inviteRequestDto.GroupAssignment))
        {
            var userId = inviteGraphResult.InvitedUser.Id ?? throw new InvalidOperationException("User ID is null");
            await _graphClient.AssignUserGroup(userId, inviteRequestDto.GroupAssignment);
        }

        var newUser = new User
        {
            Id = Guid.Parse(inviteGraphResult.InvitedUser.Id),
            Name = inviteRequestDto.Name,
            Email = inviteRequestDto.Email,
            GroupAssignment = !string.IsNullOrEmpty(inviteRequestDto.GroupAssignment) ? Guid.Parse(inviteRequestDto.GroupAssignment) : null,
            IsActive = false,
            InvitedAt = DateTime.UtcNow,
            InvitationKey = invitationKey.ToString(),
        };
        await _userRepository.CreateUserAsync(newUser);

        return ServiceResult<Guid>.Ok(newUser.Id);
    }

    public async Task<ServiceResult<bool>> ValidateInviteKey(string inviteKey)
    {
        var invitedUser = await _userRepository.GetByInvitationKeyAsync(inviteKey);
        if (invitedUser == null) return ServiceResult<bool>.Ok(false);

        var inviteIsValid = !invitedUser.IsActive &&
                            invitedUser.InvitedAt.AddDays(7) >= DateTime.UtcNow;

        return ServiceResult<bool>.Ok(inviteIsValid);
    }
}
