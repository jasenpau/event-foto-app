namespace EventFoto.Data.DTOs;

public record LoginRequestDto
{
    public string Email { get; init; }
    public string Password { get; init; }
}
