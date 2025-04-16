namespace EventFoto.Data.DTOs;

public record EventPhotoSearchParams : BaseSearchParams
{
    public int EventId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
