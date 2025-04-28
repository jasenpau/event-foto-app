namespace EventFoto.Data.Models;

public record PhotographerAssignment
{
    public int Id { get; set; }
    public int GalleryId { get; set; }
    public Guid UserId { get; set; }

    public Gallery Gallery { get; set; }
    public User User { get; set; }
}
