using EventFoto.API;
using EventFoto.Core.GraphClient;
using EventFoto.Data;
using EventFoto.Tests.TestConstants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace EventFoto.Tests;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddJsonFile("appsettings.test.json");
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<EventFotoContext>>();
            services.AddDbContext<EventFotoContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

            services.RemoveAll(typeof(IGraphClientService));
            var graphClientMock = new Mock<IGraphClientService>();
            graphClientMock.Setup(x => x.InviteUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(UserConstants.GetMockInvitationResponse());
            graphClientMock.Setup(x => x.AssignUserGroup(It.IsAny<string>(), It.IsAny<string>()));
            services.AddSingleton(graphClientMock.Object);
        });

        builder.UseEnvironment("Development");
    }
}
