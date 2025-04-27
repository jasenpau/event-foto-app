using EventFoto.Data.DatabaseProjections;

namespace EventFoto.Data.DTOs;

public record EventListDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool IsArchived { get; init; }
    public string Thumbnail { get; init; }
    public int PhotoCount { get; init; }

    public static EventListDto FromProjection(EventListProjection eventData) => new()
    {
        Id = eventData.Id,
        Name = eventData.Name,
        StartDate = eventData.StartDate,
        EndDate = eventData.EndDate,
        IsArchived = eventData.IsArchived,
        Thumbnail = eventData.Filename,
        PhotoCount = eventData.PhotoCount ?? 0,
    };
}
