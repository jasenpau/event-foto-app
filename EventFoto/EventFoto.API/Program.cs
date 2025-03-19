using EventFoto.API.Filters;
using EventFoto.API.Providers;
using EventFoto.Core.Events;
using EventFoto.Core.Users;
using EventFoto.Data;
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
        
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/uploads"
        });
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IGroupSettingsProvider, GroupSettingsProvider>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IUserService, UserService>();
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
