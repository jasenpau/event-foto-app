namespace EventFoto.Data.Models;

public record UserCreateDetails
{
    public string Name { get; init; }
    public string Email { get; init; }
}
