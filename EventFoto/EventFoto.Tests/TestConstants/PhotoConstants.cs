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
            CaptureDate = DateTime.UtcNow.AddDays(-1),
            UploadDate = DateTime.UtcNow.AddDays(-1),
            UserId = UserConstants.GetTestPhotographer().Id,
            IsProcessed = true,
            ProcessedFilename = "out-test1.jpg",
        },

        new()
        {
            Id = 2,
            GalleryId = galleryId,
            Filename = "test2.jpg",
            CaptureDate = DateTime.UtcNow.AddDays(-1),
            UploadDate = DateTime.UtcNow.AddDays(-1),
            UserId = UserConstants.GetTestPhotographer().Id,
            IsProcessed = true,
            ProcessedFilename = "out-test2.jpg"
        },

        new()
        {
            Id = 3,
            GalleryId = galleryId,
            Filename = "test3.jpg",
            CaptureDate = DateTime.UtcNow.AddDays(-1),
            UploadDate = DateTime.UtcNow.AddDays(-1),
            UserId = UserConstants.GetTestPhotographer().Id,
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
