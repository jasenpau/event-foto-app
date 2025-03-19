namespace EventFoto.Data.Models;

public record AppGroups
{
    public string SystemAdministrators { get; init; }
    public string EventAdministrators { get; init; }
    public string Photographers { get; init; }
}
