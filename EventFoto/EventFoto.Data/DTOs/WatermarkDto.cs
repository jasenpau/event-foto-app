using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record WatermarkDto
{
    public int Id { get; set; }
    public string Filename { get; set; }
    public string Name { get; set; }

    public static WatermarkDto FromModel(Watermark watermark) => new()
    {
        Id = watermark.Id,
        Filename = watermark.Filename,
        Name = watermark.Name
    };
}
