using System.Net;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;

namespace EventFoto.Core.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<ServiceResult<User>> GetUser(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return ServiceResult<User>.Fail(AppErrorMessage.UserNotFound, HttpStatusCode.NotFound);
        return ServiceResult<User>.Ok(user);
    }

    public async Task<ServiceResult<User>> CreateUser(UserCreateDetails userDetails, Guid userId)
    {
        var newUser = new User
        {
            Id = userId,
            Name = userDetails.Name,
            Email = userDetails.Email,
        };
        var user = await _userRepository.CreateUserAsync(newUser);
        return ServiceResult<User>.Ok(user);
    }
}
