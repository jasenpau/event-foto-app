namespace EventFoto.Data.DTOs;

public record EventSearchParams : BaseSearchParams
{
    public string Query { get; init; } = null;
}
