using EventFoto.Data.Enums;

namespace EventFoto.Data.Models;

public record UserCredential
{
    public Guid Id { get; init; }
    public int UserId { get; init; }
    public CredentialType Type { get; init; }
    public string HashedPassword { get; init; }
    
    public User User { get; init; }
}
