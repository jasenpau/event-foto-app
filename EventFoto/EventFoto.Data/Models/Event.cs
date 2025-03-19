namespace EventFoto.Data.Models;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsArchived { get; set; }
    public DateTime StartDate { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
}
