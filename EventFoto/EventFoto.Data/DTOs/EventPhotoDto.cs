﻿using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record EventPhotoDto
{
    public int Id { get; init; }
    public string Filename { get; init; }
    public DateTime UploadDate { get; init; }
    public DateTime CaptureDate { get; init; }
    public bool IsProcessed { get; init; }
    public string ProcessedFilename { get; init; }
    public int EventId { get; init; }
    public string EventName { get; init; }
    public string GalleryName { get; init; }
    public int GalleryId { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; }

    public static EventPhotoDto FromEventPhoto(EventPhoto photo) => new()
    {
        Id = photo.Id,
        Filename = photo.Filename,
        UploadDate = photo.UploadDate,
        CaptureDate = photo.CaptureDate,
        IsProcessed = photo.IsProcessed,
        ProcessedFilename = photo.ProcessedFilename,
        EventId = photo.Gallery.EventId,
        EventName = photo.Gallery.Event.Name,
        GalleryName = photo.Gallery.Name,
        GalleryId = photo.GalleryId,
        UserId = photo.UserId,
        UserName = photo.User.Name,
    };
}
