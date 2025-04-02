namespace EventFoto.Data.DTOs;

public record BaseSearchParams
{
    private static readonly int DefaultPageSize = 20;

    public int PageSize { get; init; } = DefaultPageSize;
    public string KeyOffset { get; init; } = null;
}
