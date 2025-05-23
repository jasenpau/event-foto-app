using EventFoto.Core;
using EventFoto.Data;
using EventFoto.Processor.CleanupProcessor;
using EventFoto.Processor.DownloadZipProcessor;
using EventFoto.Processor.EventArchiveProcessor;
using EventFoto.Processor.ImageProcessor;
using EventFoto.Processor.PhotoArchiveService;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventFoto.Processor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = FunctionsApplication.CreateBuilder(args);

        builder.ConfigureFunctionsWebApplication();

        builder.Configuration
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        builder.Services.AddDbContext<EventFotoContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
        });

        ServiceConfigurator.ConfigureServices(builder.Services, builder.Configuration);
        builder.Services.AddScoped<IImageProcessor, ImageProcessor.ImageProcessor>();
        builder.Services.AddScoped<IDownloadZipProcessor, DownloadZipProcessor.DownloadZipProcessor>();
        builder.Services.AddScoped<ICleanupProcessor, CleanupProcessor.CleanupProcessor>();
        builder.Services.AddScoped<IEventArchiveProcessor, EventArchiveProcessor.EventArchiveProcessor>();
        builder.Services.AddScoped<IPhotoArchiveService, PhotoArchiveService.PhotoArchiveService>();

        // Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
        // builder.Services
        //     .AddApplicationInsightsTelemetryWorkerService()
        //     .ConfigureFunctionsApplicationInsights();

        builder.Build().Run();
    }
}
