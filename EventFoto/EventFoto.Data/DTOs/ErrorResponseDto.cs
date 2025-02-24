namespace EventFoto.Data.DTOs;

public record ErrorResponseDto
{
    public string Message { get; init; }
    public string MessageCode { get; init; }
}
