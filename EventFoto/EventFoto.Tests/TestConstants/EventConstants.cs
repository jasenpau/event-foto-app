using EventFoto.Data.Models;

namespace EventFoto.Tests.TestConstants;

public static class EventConstants
{
    public static Event GetCurrentEvent() => new()
    {
        Id = 1,
        Name = "Test Event 1",
        StartDate = DateTime.UtcNow.AddDays(-1),
        EndDate = DateTime.UtcNow.AddDays(1),
        DefaultGalleryId = 1,
        Galleries = new List<Gallery>
        {
            new()
            {
                Id = 1,
                Name = "Test Gallery 1",
                EventId = 1,
            }
        },
        IsArchived = false,
        CreatedOn = DateTime.UtcNow,
        CreatedBy = UserConstants.GetTestEventAdmin().Id,
        Watermark = new Watermark
        {
            Id = 1,
            Name = "Test watermark",
            Filename = "watermark.png",
        },
        WatermarkId = 1,
    };
}
