namespace EventFoto.Data.DTOs;

public record EventSearchParams : PagedParams
{
    public string Query { get; init; } = null;
    public DateTime? FromDate { get; init; } = null;
    public DateTime? ToDate { get; init; } = null;
    public bool? ShowArchived { get; init; } = null;
}
