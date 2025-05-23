﻿using Azure.Storage.Blobs;
using EventFoto.Core.Assignments;
using EventFoto.Core.EventPhotos;
using EventFoto.Core.Events;
using EventFoto.Core.Galleries;
using EventFoto.Core.Processing;
using EventFoto.Core.Users;
using EventFoto.Core.Watermarks;
using EventFoto.Data.BlobStorage;
using EventFoto.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventFoto.Core;

public static class ServiceConfigurator
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventPhotoRepository, EventPhotoRepository>();
        services.AddScoped<IDownloadRequestRepository, DownloadRequestRepository>();
        services.AddScoped<IGalleryRepository, GalleryRepository>();
        services.AddScoped<IUploadBatchRepository, UploadBatchRepository>();
        services.AddScoped<IWatermarkRepository, WatermarkRepository>();
        services.AddScoped<IPhotographerAssignmentRepository, PhotographerAssignmentRepository>();

        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEventPhotoService, EventPhotoService>();
        services.AddScoped<IGalleryService, GalleryService>();
        services.AddScoped<IWatermarkService, WatermarkService>();
        services.AddScoped<IAssignmentService, AssignmentService>();

        services.AddScoped<IBlobServiceClientHelper, BlobServiceClientHelper>();
        services.AddScoped<IBlobStorage, BlobStorage>();
        services.AddScoped<IProcessingQueue, ProcessingQueue>();

        services.AddSingleton<IQueueClientFactory, QueueClientFactory>();
        services.AddSingleton(x =>
            new BlobServiceClient(configuration["AzureStorage:ConnectionString"]));
    }
}
