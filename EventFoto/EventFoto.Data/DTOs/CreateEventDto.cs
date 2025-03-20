using EventFoto.Data.Attributes;

namespace EventFoto.Data.DTOs;

public record CreateEventDto
{
    [AppRequired]
    public string Name { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string Location { get; init; }
    public string Note { get; init;}
}
