using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record UserDto {
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public Guid? GroupAssignment { get; init; }
    public bool IsActive { get; init; }

    public static UserDto FromModel(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        GroupAssignment = user.GroupAssignment,
        IsActive = user.IsActive,
    };
}
