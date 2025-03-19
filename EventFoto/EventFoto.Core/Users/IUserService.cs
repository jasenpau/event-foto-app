using EventFoto.Data.Models;

namespace EventFoto.Core.Users;

public interface IUserService
{
    public Task<ServiceResult<User>> GetUser(Guid userId);
    public Task<ServiceResult<User>> CreateUser(UserCreateDetails userDetails, Guid userId);
}
