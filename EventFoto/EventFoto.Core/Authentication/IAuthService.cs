using EventFoto.Data.Models;

namespace EventFoto.Core.Authentication;

public interface IAuthService
{
    public Task<ServiceResult<string>> LoginAsync(string email, string password);
    public Task<ServiceResult<string>> RegisterPasswordAsync(UserCreateDetails userDetails, string password);
}
