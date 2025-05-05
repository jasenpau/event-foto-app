using Microsoft.Graph.Models;
using User = EventFoto.Data.Models.User;

namespace EventFoto.Tests.TestConstants;

public static class UserConstants
{
    public static Guid PhotographerGroupId => Guid.Parse("30bd2af2-ca84-4d1d-8d22-2b4534def269");
    public static Guid EventAdminGroupId => Guid.Parse("38b523db-701d-43d5-9921-2f3aecb2d35e");
    public static Guid SystemAdminGroupId => Guid.Parse("81532fbd-a60b-44c7-87ab-4cff99fbe612");

    public static User GetTestViewer() => new()
    {
        Id = Guid.Parse("2a4bf25d-be20-4c9a-9f35-eadb6ef766c9"),
        Name = "Test Viewer",
        Email = "test1@example.com",
        IsActive = true,
    };

    public static User GetTestPhotographer() => new()
    {
        Id = Guid.Parse("3b5bf25d-be20-4c9a-9f35-eadb6ef766c0"),
        Name = "Test Photographer",
        Email = "photographer@example.com",
        GroupAssignment = PhotographerGroupId,
        IsActive = true,
    };

    public static User GetTestEventAdmin() => new()
    {
        Id = Guid.Parse("d6cfa12f-4e6f-41bc-bfae-0b8c7eb71d98"),
        Name = "Test Event Admin",
        Email = "event.admin@example.com",
        GroupAssignment = EventAdminGroupId,
        IsActive = true,
    };

    public static User GetTestSystemAdmin() => new()
    {
        Id = Guid.Parse("ac0cb6d0-c6ee-43a7-9afc-9b8a0f5b04c4"),
        Name = "Test System Admin",
        Email = "system.admin@example.com",
        GroupAssignment = SystemAdminGroupId,
        IsActive = true,
    };

    public static User GetInvitedUser() => new()
    {
        Id = Guid.Parse("5b8b2c1c-6ac1-467b-8ffa-7d1cdc15d2dd"),
        Name = "Invited User",
        Email = "invited@example.com",
        InvitationKey = "3a7d9a51-d8dd-42ad-9328-28b4aaaa86cc",
        InvitedAt = DateTime.UtcNow,
        IsActive = false,
    };

    public static Invitation GetMockInvitationResponse() => new()
    {
        InvitedUser = new()
        {
            Id = "3e8ea332-8fa7-44c5-8261-fc7a7d42b73f",
        }
    };
}
