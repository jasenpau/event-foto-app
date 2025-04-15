using EventFoto.Core.EventPhotos;
using EventFoto.Core.Events;
using EventFoto.Core.PhotoProcessing;
using EventFoto.Core.Users;
using EventFoto.Data.PhotoStorage;
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

        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEventPhotoService, EventPhotoService>();

        services.AddScoped<IPhotoBlobStorage, PhotoBlobStorage>();
        services.AddScoped<IPhotoProcessingQueue, PhotoProcessingQueue>();
    }
}
