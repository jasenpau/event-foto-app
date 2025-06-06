﻿using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record UserListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public Guid? GroupAssignment { get; init; }
    public bool IsActive { get; init; }
    public DateTime? InvitedAt { get; init; }

    public static UserListDto FromUser(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        GroupAssignment = user.GroupAssignment,
        IsActive = user.IsActive,
        InvitedAt = user.InvitedAt
    };
}
