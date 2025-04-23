using EventFoto.Data.DatabaseProjections;
using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record GalleryDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public int EventId { get; init; }
    public string Filename { get; init; }
    public int PhotoCount { get; init; }

    public static GalleryDto FromModel(Gallery gallery) => new()
    {
        Id = gallery.Id,
        Name = gallery.Name,
        EventId = gallery.EventId,
        Filename = null,
        PhotoCount = 0,
    };

    public static GalleryDto FromProjection(EventGalleryProjection projection) => new()
    {
        Id = projection.Id,
        Name = projection.Name,
        EventId = projection.EventId,
        Filename = projection.Filename,
        PhotoCount = projection.PhotoCount ?? 0,
    };
}
