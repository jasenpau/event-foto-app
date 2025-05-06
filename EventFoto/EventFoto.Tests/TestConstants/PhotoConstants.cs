using EventFoto.Data.Models;

namespace EventFoto.Tests.TestConstants;

public static class PhotoConstants
{
    public static List<EventPhoto> GetTestPhotos(int galleryId) =>
    [
        new()
        {
            Id = 1,
            GalleryId = galleryId,
            Filename = "test1.jpg",
            CaptureDate = DateTime.UtcNow.AddMinutes(-6),
            UploadDate = DateTime.UtcNow.AddMinutes(-6),
            UserId = UserConstants.GetTestPhotographer().Id,
            IsProcessed = true,
            ProcessedFilename = "out-test1.jpg",
        },

        new()
        {
            Id = 2,
            GalleryId = galleryId,
            Filename = "test2.jpg",
            CaptureDate = DateTime.UtcNow.AddMinutes(-5),
            UploadDate = DateTime.UtcNow.AddMinutes(-5),
            UserId = UserConstants.GetTestPhotographer().Id,
            IsProcessed = true,
            ProcessedFilename = "out-test2.jpg"
        },

        new()
        {
            Id = 3,
            GalleryId = galleryId,
            Filename = "test3.jpg",
            CaptureDate = DateTime.UtcNow.AddMinutes(-4),
            UploadDate = DateTime.UtcNow.AddMinutes(-4),
            UserId = UserConstants.GetTestPhotographer().Id,
        },

        new()
        {
            Id = 4,
            GalleryId = galleryId,
            Filename = "test4.jpg",
            CaptureDate = DateTime.UtcNow.AddMinutes(-3),
            UploadDate = DateTime.UtcNow.AddMinutes(-3),
            UserId = UserConstants.GetTestPhotographer().Id,
            IsProcessed = true,
            ProcessedFilename = "out-test4.jpg"
        },

        new()
        {
            Id = 5,
            GalleryId = galleryId,
            Filename = "test5.jpg",
            CaptureDate = DateTime.UtcNow.AddMinutes(-2),
            UploadDate = DateTime.UtcNow.AddMinutes(-2),
            UserId = UserConstants.GetTestPhotographer().Id,
            IsProcessed = true,
            ProcessedFilename = "out-test5.jpg"
        },

        new()
        {
            Id = 6,
            GalleryId = galleryId,
            Filename = "test6.jpg",
            CaptureDate = DateTime.UtcNow.AddMinutes(-1),
            UploadDate = DateTime.UtcNow.AddMinutes(-1),
            UserId = UserConstants.GetTestPhotographer().Id,
            IsProcessed = true,
            ProcessedFilename = "out-test6.jpg"
        }
    ];


    public static UploadBatch GetTestUploadBatch() => new()
    {
        Id = 1,
        UserId = UserConstants.GetTestPhotographer().Id
    };

    public static DownloadRequest GetTestDownloadRequest() => new()
    {
        Id = 1,
        UserId = UserConstants.GetTestPhotographer().Id,
        DownloadProcessedPhotos = true,
        DownloadImages = GetTestPhotos(1).Select(x => new DownloadImage()
        {
            DownloadRequestId = 1,
            EventPhotoId = x.Id
        }).ToList(),
    };
}
