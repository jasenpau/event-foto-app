using EventFoto.Core.EventPhotos;
using EventFoto.Core.Events;
using EventFoto.Core.Galleries;
using EventFoto.Core.PhotoProcessing;
using EventFoto.Core.Processing;
using EventFoto.Core.Users;
using EventFoto.Data.BlobStorage;
using EventFoto.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EventFoto.Core;

public static class ServiceConfigurator
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventPhotoRepository, EventPhotoRepository>();
        services.AddScoped<IDownloadRequestRepository, DownloadRequestRepository>();
        services.AddScoped<IGalleryRepository, GalleryRepository>();

        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEventPhotoService, EventPhotoService>();
        services.AddScoped<IGalleryService, GalleryService>();

        services.AddScoped<IBlobStorage, BlobStorage>();
        services.AddScoped<IProcessingQueue, ProcessingQueue>();
    }
}
