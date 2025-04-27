namespace EventFoto.Data.DatabaseProjections;

public record EventListProjection
{
    public int Id { get; init; }
    public string Name { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool IsArchived { get; init; }
    public string Filename { get; init; }
    public int? PhotoCount { get; init; }
}
