using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record EventDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsArchived { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }

    public static EventDto FromEvent(Event eventData)
    {
        return new EventDto
        {
            Id = eventData.Id,
            Name = eventData.Name,
            IsArchived = eventData.IsArchived,
            CreatedBy = eventData.CreatedBy,
            CreatedOn = eventData.CreatedOn,

        };
    }
}
