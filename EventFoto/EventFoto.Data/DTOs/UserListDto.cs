using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record UserListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }

    public static UserListDto FromUser(User user)
    {
        return new UserListDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
        };
    }
}
