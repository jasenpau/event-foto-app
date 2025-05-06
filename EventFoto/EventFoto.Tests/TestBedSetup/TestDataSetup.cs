using System.Security.Claims;
using EventFoto.Data;
using EventFoto.Data.Models;
using EventFoto.Tests.TestConstants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Xunit.Sdk;

namespace EventFoto.Tests.TestBedSetup;

public class TestDataSetup
{
    private readonly TestApplicationFactory _factory;

    public TestDataSetup(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task<HttpClient> SetupEmpty()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();
        await context.Database.EnsureDeletedAsync();

        return _factory.CreateClient();
    }

    public async Task<HttpClient> SetupInvalidUserId()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();
        await context.Database.EnsureDeletedAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Invalid User"),
            new(ClaimConstants.ObjectId, "invalid-guid"),
            new("groups", "invalid-group"),
        };

        var claimsProvider = new UserClaimsProvider(claims);
        var client = _factory.CreateClientWithTestAuth(claimsProvider);

        return client;
    }

    public async Task<HttpClient> SetupWithUser(User user)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();
        await context.Database.EnsureDeletedAsync();

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var claimsProvider = UserClaimsProvider.WithUser(user);
        var client = _factory.CreateClientWithTestAuth(claimsProvider);

        return client;
    }

    public Task AddUser(User user)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        return context.AddUser(user);
    }

    public Task AddEvent(Event eventData)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        return context.AddEvent(eventData);
    }

    public Task AssignPhotographer(int galleryId, Guid photographerId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        return context.AssignPhotographer(galleryId, photographerId);
    }

    public Task AddGallery(Gallery gallery)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        return context.AddGallery(gallery);
    }

    public Task AddPhotos(List<EventPhoto> photos)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        return context.AddPhotos(photos);
    }

    public Task AddPhoto(EventPhoto testPhoto) => AddPhotos([testPhoto]);

    public Task AddUploadBatch(UploadBatch batch)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        return context.AddUploadBatch(batch);
    }

    public Task AddDownloadRequest(DownloadRequest request)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        return context.AddDownloadRequest(request);
    }

    public Task AddWatermark(Watermark watermark)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        return context.AddWatermark(watermark);
    }

    public Task AddWatermarks(List<Watermark> watermarks)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        return context.AddWatermarks(watermarks);
    }
}
