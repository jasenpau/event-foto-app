namespace EventFoto.Data.DTOs;

public record WatermarkSearchParams : PagedParams
{
    public string Query { get; init; }
}
