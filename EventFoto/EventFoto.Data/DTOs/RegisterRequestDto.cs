namespace EventFoto.Data.DTOs;

public record RegisterRequestDto
{
    public string Name { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
}
