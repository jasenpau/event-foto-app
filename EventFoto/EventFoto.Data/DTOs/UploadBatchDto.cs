namespace EventFoto.Data.DTOs;

public record UploadBatchDto
{
    public int Id { get; init; }
    public int PhotoCount { get; init; }
    public bool Ready { get; init; }
}
