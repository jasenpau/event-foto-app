namespace EventFoto.Data.DTOs;

public record CreateEditEventDto
{
    public string Name { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string Location { get; init; }
    public string Note { get; init;}
    public int? WatermarkId { get; init; }
    public bool ReprocessPhotos { get; init; } = false;
}
