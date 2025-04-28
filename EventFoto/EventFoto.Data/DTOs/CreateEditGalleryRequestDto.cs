namespace EventFoto.Data.DTOs;

public record CreateEditGalleryRequestDto
{
    public string Name { get; set; }
    public int? WatermarkId { get; set; }
    public bool ReprocessPhotos { get; set; } = false;
}
