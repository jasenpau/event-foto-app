using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Core.Users;

public interface IUserService
{
    public Task<ServiceResult<User>> GetUserAsync(Guid userId);
    public Task<ServiceResult<User>> InviteRegisterAsync(RegisterDto registerDto, Guid userId);
    public Task<ServiceResult<PagedData<string, User>>> SearchUsersAsync(UserSearchParams searchParams);
    public Task<ServiceResult<Guid>> InviteUserAsync(UserInviteRequestDto inviteRequestDto);
    public Task<ServiceResult<bool>> ValidateInviteKey(string inviteKey);
}
