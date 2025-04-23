namespace EventFoto.Data.DTOs;

public record EventPhotoSearchParams : PagedParams
{
    public int? GalleryId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
