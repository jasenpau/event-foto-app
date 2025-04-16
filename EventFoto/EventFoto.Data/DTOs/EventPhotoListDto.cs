using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record EventPhotoListDto
{
    public int Id { get; init; }
    public bool IsProcessed { get; set; }
    public string ProcessedFilename { get; set; }
    public DateTime CaptureDate { get; set; }

    public static EventPhotoListDto FromEventPhoto(EventPhoto eventPhoto)
    {
        return new EventPhotoListDto
        {
            Id = eventPhoto.Id,
            IsProcessed = eventPhoto.IsProcessed,
            ProcessedFilename = eventPhoto.ProcessedFilename,
            CaptureDate = eventPhoto.CaptureDate,
        };
    }
}
