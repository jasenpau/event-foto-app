using EventFoto.Data;
using EventFoto.Data.Models;

namespace EventFoto.Tests.TestBedSetup;

public static class EventFotoContextExtensions
{
    public static async Task AddUser(this EventFotoContext context, User user)
    {
       context.Users.Add(user);
       await context.SaveChangesAsync();
    }

    public static async Task AddEvent(this EventFotoContext context, Event eventData)
    {
       context.Events.Add(eventData);
       await context.SaveChangesAsync();
    }

    public static async Task AssignPhotographer(this EventFotoContext context, int galleryId, Guid photographerId)
    {
       context.PhotographerAssignments.Add(new PhotographerAssignment { GalleryId = galleryId, UserId = photographerId });
       await context.SaveChangesAsync();
    }

    public static async Task AddGallery(this EventFotoContext context, Gallery gallery)
    {
       context.Galleries.Add(gallery);
       await context.SaveChangesAsync();
    }

    public static async Task AddPhotos(this EventFotoContext context, List<EventPhoto> photos)
    {
       context.EventPhotos.AddRange(photos);
       await context.SaveChangesAsync();
    }

    public static Task AddPhoto(this EventFotoContext context, EventPhoto testPhoto) =>
        context.AddPhotos(new List<EventPhoto> { testPhoto });

    public static async Task AddUploadBatch(this EventFotoContext context, UploadBatch batch)
    {
       context.UploadBatches.Add(batch);
       await context.SaveChangesAsync();
    }

    public static async Task AddDownloadRequest(this EventFotoContext context, DownloadRequest request)
    {
       context.DownloadRequests.Add(request);
       await context.SaveChangesAsync();
    }

    public static async Task AddWatermark(this EventFotoContext context, Watermark watermark)
    {
       context.Watermarks.Add(watermark);
       await context.SaveChangesAsync();
    }

    public static async Task AddWatermarks(this EventFotoContext context, List<Watermark> watermarks)
    {
        context.Watermarks.AddRange(watermarks);
        await context.SaveChangesAsync();
    }
}
