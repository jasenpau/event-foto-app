using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Core.Users;

public interface IUserService
{
    public Task<ServiceResult<User>> GetUserAsync(Guid userId);
    public Task<ServiceResult<User>> CreateUserAsync(UserCreateDetails userDetails, Guid userId);
    public Task<ServiceResult<PagedData<string, User>>> SearchUsersAsync(UserSearchParams searchParams);
}
