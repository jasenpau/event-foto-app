namespace EventFoto.Data.DTOs;

public record UserSearchParams : PagedParams
{
    public string Query { get; init; } = null;
    public int? ExcludeEventId { get; init; } = null;
}
