using EventFoto.Data.DatabaseProjections;
using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record GalleryDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public int EventId { get; init; }
    public bool? IsMainGallery { get; init; }
    public string Thumbnail { get; init; }
    public int PhotoCount { get; init; }
    public int? WatermarkId { get; init; }

    public static GalleryDto FromModel(Gallery gallery) => new()
    {
        Id = gallery.Id,
        Name = gallery.Name,
        IsMainGallery = gallery.Event?.DefaultGalleryId == gallery.Id,
        EventId = gallery.EventId,
        Thumbnail = null,
        PhotoCount = 0,
        WatermarkId = gallery.WatermarkId
    };

    public static GalleryDto FromProjection(EventGalleryProjection projection) => new()
    {
        Id = projection.Id,
        Name = projection.Name,
        EventId = projection.EventId,
        Thumbnail = projection.Filename,
        PhotoCount = projection.PhotoCount ?? 0,
        IsMainGallery = projection.DefaultGalleryId == projection.Id,
        WatermarkId = null
    };
}
