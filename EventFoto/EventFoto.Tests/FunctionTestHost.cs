using Azure.Storage.Blobs;
using EventFoto.Core;
using EventFoto.Core.Processing;
using EventFoto.Data;
using EventFoto.Data.BlobStorage;
using EventFoto.Processor.CleanupProcessor;
using EventFoto.Processor.DownloadZipProcessor;
using EventFoto.Processor.EventArchiveProcessor;
using EventFoto.Processor.ImageProcessor;
using EventFoto.Processor.PhotoArchiveService;
using EventFoto.Tests.TestBedSetup;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace EventFoto.Tests;

public class FunctionTestHost
{
    public IServiceProvider Services { get; }
    public IConfiguration Configuration { get; }

    public FunctionTestHost(Action<IServiceCollection>? overrideServices = null)
    {
        var services = new ServiceCollection();
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("appsettings.test.json");
        Configuration = configBuilder.Build();
        services.AddSingleton(Configuration);

        services.AddLogging();

        services.AddDbContext<EventFotoContext>(options =>
            options.UseInMemoryDatabase("TestDatabaseFunctions")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

        ServiceConfigurator.ConfigureServices(services, Configuration);

        services.RemoveAll<BlobServiceClient>();
        services.RemoveAll<IBlobServiceClientHelper>();
        services.AddSingleton(BlobServiceClientMock.GetMock().Object);
        services.AddSingleton(BlobServiceClientMock.GetBlobHelperMock().Object);

        services.RemoveAll<IQueueClientFactory>();
        services.AddSingleton(QueueClientMock.GetFactoryMock().Object);

        services.AddScoped<IImageProcessor, ImageProcessor>();
        services.AddScoped<IDownloadZipProcessor, DownloadZipProcessor>();
        services.AddScoped<ICleanupProcessor, CleanupProcessor>();
        services.AddScoped<IEventArchiveProcessor, EventArchiveProcessor>();
        services.AddScoped<IPhotoArchiveService, PhotoArchiveService>();

        var functionContextMock = new Mock<FunctionContext>();
        services.AddSingleton<FunctionContext>(functionContextMock.Object);

        overrideServices?.Invoke(services);

        Services = services.BuildServiceProvider();
    }

    public T GetService<T>() where T : class => Services.GetRequiredService<T>();
    public EventFotoContext GetDbContext() => Services.GetRequiredService<EventFotoContext>();
}
