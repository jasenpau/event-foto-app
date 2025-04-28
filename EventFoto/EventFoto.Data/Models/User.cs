namespace EventFoto.Data.Models;

public record User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public Guid? GroupAssignment { get; set; }
    public bool IsActive { get; set; }
    public DateTime InvitedAt { get; set; }
    public string InvitationKey { get; set; }

    public IList<PhotographerAssignment> Assignments { get; set; }
}
