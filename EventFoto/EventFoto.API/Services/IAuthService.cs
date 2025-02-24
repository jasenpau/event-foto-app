using EventFoto.Data.Models;

namespace EventFoto.API.Services;

public interface IAuthService
{
    public Task<ServiceResult<string>> LoginAsync(string email, string password);
}
