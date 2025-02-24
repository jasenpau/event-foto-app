namespace EventFoto.Data.Models;

public record User
{
    public int Id { get; init; }
    public string Email { get; init; }
    public string Name { get; init; }
    
    public IList<UserCredential> Credentials { get; init; }
}
