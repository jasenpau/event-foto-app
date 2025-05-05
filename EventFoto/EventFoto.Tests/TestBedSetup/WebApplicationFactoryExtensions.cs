using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace EventFoto.Tests.TestBedSetup;

public static class WebApplicationFactoryExtensions
{
    public static WebApplicationFactory<T> WithAuthentication<T>(this WebApplicationFactory<T> factory, UserClaimsProvider claimsProvider) where T : class
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                services.AddScoped<UserClaimsProvider>(_ => claimsProvider);
            });
        });
    }

    public static HttpClient CreateClientWithTestAuth<T>(this WebApplicationFactory<T> factory, UserClaimsProvider claimsProvider) where T : class
    {
        var client = factory.WithAuthentication(claimsProvider).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test_token");

        return client;
    }
}
