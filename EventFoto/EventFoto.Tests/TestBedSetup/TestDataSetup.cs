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

    public async Task AddUser(User user)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    public async Task AddEvent(Event eventData)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        context.Events.Add(eventData);
        await context.SaveChangesAsync();
    }

    public async Task AssignPhotographer(int galleryId, Guid photographerId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        context.PhotographerAssignments.Add(new PhotographerAssignment { GalleryId = galleryId, UserId = photographerId });
        await context.SaveChangesAsync();
    }

    public async Task AddGallery(Gallery gallery)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        context.Galleries.Add(gallery);
        await context.SaveChangesAsync();
    }

    public async Task AddPhotos(IList<EventPhoto> photos)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        context.EventPhotos.AddRange(photos);
        await context.SaveChangesAsync();
    }

    public Task AddPhoto(EventPhoto testPhoto) =>
        AddPhotos(new List<EventPhoto> { testPhoto });

    public async Task AddUploadBatch(UploadBatch batch)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        context.UploadBatches.Add(batch);
        await context.SaveChangesAsync();
    }

    public async Task AddDownloadRequest(DownloadRequest request)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventFotoContext>();

        context.DownloadRequests.Add(request);
        await context.SaveChangesAsync();
    }
}
