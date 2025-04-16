using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record EventDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsArchived { get; set; }
    public string Location { get; set; }
    public string Note { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; }
    public DateTime CreatedOn { get; set; }

    public static EventDto FromEvent(Event eventData) => new()
    {
        Id = eventData.Id,
        Name = eventData.Name,
        StartDate = eventData.StartDate,
        EndDate = eventData.EndDate,
        Note = eventData.Note,
        Location = eventData.Location,
        IsArchived = eventData.IsArchived,
        CreatedBy = eventData.CreatedBy,
        CreatedOn = eventData.CreatedOn,
        CreatedByName = eventData.CreatedByUser.Name,
    };
}
