using System.Reflection;
using EventFoto.Core;
using EventFoto.Core.Events;
using EventFoto.Core.Filters;
using EventFoto.Core.Providers;
using EventFoto.Core.Users;
using EventFoto.Data;
using EventFoto.Data.PhotoStorage;
using EventFoto.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
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
            .AddJsonFile("appsettings.local.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });
        
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        builder.Services.AddDbContextPool<EventFotoContext>(opt => 
            opt.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

        ConfigureServices(builder.Services);
        ConfigureAuthentication(builder.Services, builder.Configuration);

        var app = builder.Build();

        // app.UseHttpsRedirection();
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(builder.Environment.ContentRootPath, "Thumbnails")),
            RequestPath = "/thumbnails",
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=86400");
            }
        });
        
        app.MapControllers();
        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        ServiceConfigurator.ConfigureServices(services);

        services.AddSingleton<IGroupSettingsProvider, GroupSettingsProvider>();
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
