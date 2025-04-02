namespace EventFoto.Data.DTOs;

public record UserSearchParams : BaseSearchParams
{
    public string Query { get; init; } = null;
    public int? ExcludeEventId { get; init; } = null;
}
