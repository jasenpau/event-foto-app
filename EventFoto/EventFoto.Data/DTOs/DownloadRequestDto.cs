using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record DownloadRequestDto
{
    public int Id { get; set; }
    public string Filename { get; init; }
    public Guid UserId { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public bool IsReady { get; set; }

    public static DownloadRequestDto FromModel(DownloadRequest model) => new()
    {
        Id = model.Id,
        Filename = model.Filename,
        UserId = model.UserId,
        ProcessedOn = model.ProcessedOn,
        IsReady = model.IsReady
    };
}
