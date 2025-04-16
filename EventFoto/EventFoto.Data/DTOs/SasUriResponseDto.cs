namespace EventFoto.Data.DTOs;

public record SasUriResponseDto
{
    public string SasUri { get; set; }
    public DateTime ExpiresOn { get; set; }
    public int EventId { get; set; }
}
