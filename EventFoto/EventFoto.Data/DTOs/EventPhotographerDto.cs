namespace EventFoto.Data.DTOs;

public record EventPhotographerDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public int PhotoCount { get; init; }
}
