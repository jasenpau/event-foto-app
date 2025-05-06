using EventFoto.Data.Models;

namespace EventFoto.Tests.TestConstants;

public static class GalleryConstants
{
    public static Gallery GetTestGallery(int id) => new()
    {
        Id = id,
        Name = $"Test Gallery {id}",
        EventId = 1,
    };
}
