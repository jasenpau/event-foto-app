using System.Reflection;
using Azure.Identity;
using EventFoto.API.Filters;
using EventFoto.Core;
using EventFoto.Core.CalendarExport;
using EventFoto.Core.Providers;
using EventFoto.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace EventFoto.API;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        builder.Configuration.Sources.Clear();
        builder.Configuration
            .SetBasePath(basePath!)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        builder.Services.AddControllers();

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        builder.Services.AddDbContextPool<EventFotoContext>(opt =>
        {
            opt.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
            opt.EnableSensitiveDataLogging();
        });

        ConfigureServices(builder.Services, builder.Configuration);
        ConfigureAuthentication(builder.Services, builder.Configuration);

        var app = builder.Build();

        // app.UseHttpsRedirection();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        ServiceConfigurator.ConfigureServices(services);

        services.AddScoped<ICalendarExportService, CalendarExportService>();
        services.AddSingleton<IGroupSettingsProvider, GroupSettingsProvider>();

        services.AddSingleton<GraphServiceClient>(_ =>
        {
            var clientId = configuration["AzureAd:ClientId"];
            var tenantId = configuration["AzureAd:TenantId"];
            var clientSecret = configuration["AzureAd:ClientSecret"];

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            return new GraphServiceClient(credential);
        });
    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(options =>
            {
                configuration.Bind("AzureAd", options);
            }, options =>
            {
                configuration.Bind("AzureAd", options);
            });

        services.AddAuthorization();
    }
}
