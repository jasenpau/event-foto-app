namespace EventFoto.Data.Models;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsArchived { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Location { get; set; }
    public string Note { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    
    public User CreatedByUser { get; set; }
    public IList<User> Photographers { get; set; }
    public IList<EventPhoto> Photos { get; set; }
}
