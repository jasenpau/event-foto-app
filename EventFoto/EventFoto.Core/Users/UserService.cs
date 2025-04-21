using System.Net;
using EventFoto.Data.DTOs;
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
    
    public async Task<ServiceResult<User>> GetUserAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return ServiceResult<User>.Fail("User not found", HttpStatusCode.NotFound);
        return ServiceResult<User>.Ok(user);
    }

    public async Task<ServiceResult<User>> CreateUserAsync(UserCreateDetails userDetails, Guid userId)
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

    public async Task<ServiceResult<PagedData<string, User>>> SearchUsersAsync(UserSearchParams searchParams)
    {
        var result = await _userRepository.SearchUsersAsync(searchParams);
        return result is not null
            ? ServiceResult<PagedData<string, User>>.Ok(result)
            : ServiceResult<PagedData<string, User>>.Fail("Query failed", HttpStatusCode.InternalServerError);
    }
}
