namespace EventFoto.Data.Models;

public record Gallery
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int EventId { get; set; }

    public Event Event { get; set; }
    public IList<EventPhoto> Photos { get; set; }
}
