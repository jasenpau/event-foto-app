using System.ComponentModel.DataAnnotations;

namespace EventFoto.Data.DTOs;

public record AssignmentRequestDto
{
    [Required]
    public Guid UserId { get; init; }

    [Required]
    public int GalleryId { get; init; }
}
