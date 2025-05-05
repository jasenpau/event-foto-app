using EventFoto.Data;
using EventFoto.Data.Models;
using Microsoft.Extensions.DependencyInjection;

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
}
