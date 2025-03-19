namespace EventFoto.Data.DTOs;

public record RegisterDto
{
    public string Email { get; init; }
    public string Name { get; init; }
}
