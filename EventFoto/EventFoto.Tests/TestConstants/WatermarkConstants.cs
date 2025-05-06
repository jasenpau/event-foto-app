using EventFoto.Data.Models;

namespace EventFoto.Tests.TestConstants;

public static class WatermarkConstants
{
    public static Watermark GetTestWatermark() => new()
    {
        Id = 1,
        Name = "Test Watermark",
        Filename = "test-watermark.png"
    };

    public static List<Watermark> GetTestWatermarks() => new()
    {
        new Watermark
        {
            Id = 2,
            Name = "Test Watermark 2",
            Filename = "test-watermark-2.png"
        },
        new Watermark
        {
            Id = 3,
            Name = "Test Watermark 3",
            Filename = "test-watermark-3.png"
        },
        new Watermark
        {
            Id = 4,
            Name = "Test Watermark 4",
            Filename = "test-watermark-4.png"
        },
        new Watermark
        {
            Id = 5,
            Name = "Test Watermark 5",
            Filename = "test-watermark-5.png"
        },
        new Watermark
        {
            Id = 6,
            Name = "Test Watermark 6",
            Filename = "test-watermark-6.png"
        }
    };
}
