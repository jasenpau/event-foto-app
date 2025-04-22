using EventFoto.Core;
using EventFoto.Data;
using EventFoto.Processor.CleanupProcessor;
using EventFoto.Processor.DownloadZipProcessor;
using EventFoto.Processor.ImageProcessor;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddDbContextPool<EventFotoContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));

});

ServiceConfigurator.ConfigureServices(builder.Services);
builder.Services.AddScoped<IImageProcessor, ImageProcessor>();
builder.Services.AddScoped<IDownloadZipProcessor, DownloadZipProcessor>();
builder.Services.AddScoped<ICleanupProcessor, CleanupProcessor>();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
