namespace EventFoto.Data.Models;

public record Watermark
{
    public int Id { get; set; }
    public string Filename { get; set; }
    public string Name { get; set; }
}
