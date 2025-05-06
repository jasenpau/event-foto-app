namespace EventFoto.Data.DTOs;

public record EventPhotoSearchParams : PagedParams
{
    public int? GalleryId { get; init; }
}
