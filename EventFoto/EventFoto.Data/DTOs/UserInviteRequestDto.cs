namespace EventFoto.Data.DTOs;

public record UserInviteRequestDto
{
    public string Name { get; init; }
    public string Email { get; init; }
    public string GroupAssignment { get; init; }
}
