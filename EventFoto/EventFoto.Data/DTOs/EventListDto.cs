using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record EventListDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public DateTime StartDate { get; init; }
    public bool IsArchived { get; init; }

    public static EventListDto FromEvent(Event eventData)
    {
        return new EventListDto()
        {
            Id = eventData.Id,
            Name = eventData.Name,
            StartDate = eventData.StartDate,
            IsArchived = eventData.IsArchived
        };
    }
}
