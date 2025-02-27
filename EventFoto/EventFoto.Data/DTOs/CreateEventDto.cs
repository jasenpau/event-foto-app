using EventFoto.Data.Attributes;

namespace EventFoto.Data.DTOs;

public record CreateEventDto
{
    [AppRequired]
    public string Name { get; init; }
}
