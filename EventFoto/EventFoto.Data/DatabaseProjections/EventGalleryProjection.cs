namespace EventFoto.Data.DatabaseProjections;

public record EventGalleryProjection
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int EventId { get; set; }
    public string Filename { get; set; }
    public int? PhotoCount { get; set; }
}

