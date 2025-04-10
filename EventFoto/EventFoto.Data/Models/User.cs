namespace EventFoto.Data.Models;

public record User
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string Name { get; init; }
    public Guid GroupAssignment { get; init; }

    public IList<Event> AssignedPhotographerEvents { get; init; }
}
