using System.Net;
using System.Net.Http.Json;
using EventFoto.Data;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using EventFoto.Tests.TestBedSetup;
using EventFoto.Tests.TestConstants;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EventFoto.Tests.ControllerTests;

public class ImageControllerTests : IClassFixture<TestApplicationFactory>, IDisposable
{
    private readonly TestApplicationFactory _factory;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly TestDataSetup _testSetup;
    private readonly IServiceScope _scope;

    public ImageControllerTests(TestApplicationFactory factory,
        ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
        _testSetup = new TestDataSetup(factory);
        _scope = _factory.Services.CreateScope();
    }

    [Fact]
    public async Task GetPhotoById_WithValidId_ReturnsPhoto()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        await _testSetup.AddEvent(EventConstants.GetCurrentEvent());
        var testPhoto = PhotoConstants.GetTestPhotos(1).First();
        await _testSetup.AddPhoto(testPhoto);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/image/details/{testPhoto.Id}");

        // Act
        var result = await client.SendRequest<EventPhotoDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testPhoto.Id);
        result.Filename.Should().Be(testPhoto.Filename);
    }

    [Fact]
    public async Task GetPhotoById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        await _testSetup.AddEvent(EventConstants.GetCurrentEvent());
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/image/details/999");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BulkDelete_WithValidIds_ReturnsDeletedCount()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        await _testSetup.AddEvent(EventConstants.GetCurrentEvent());
        var testPhotos = PhotoConstants.GetTestPhotos(1);
        await _testSetup.AddPhotos(testPhotos);
        var dto = new BulkPhotoModifyDto { PhotoIds = testPhotos.Select(p => p.Id).ToList() };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/image/bulk-delete")
        {
            Content = JsonContent.Create(dto)
        };

        // Act
        var result = await client.SendRequest<int>(request);

        // Assert
        result.Should().Be(testPhotos.Count);
    }

    [Fact]
    public async Task BulkDelete_WithInvalidIds_ReturnsZero()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        var dto = new BulkPhotoModifyDto { PhotoIds = new List<int> { 999, 1000 } };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/image/bulk-delete")
        {
            Content = JsonContent.Create(dto)
        };

        // Act
        var result = await client.SendRequest<int>(request);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task BulkDownload_WithValidIds_ReturnsDownloadRequest()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        var testPhotos = PhotoConstants.GetTestPhotos(1);
        await _testSetup.AddPhotos(testPhotos);
        var dto = new BulkPhotoDownloadDto
        {
            PhotoIds = testPhotos.Select(p => p.Id).ToList(),
            Processed = true,
            Quality = 80
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/image/bulk-download")
        {
            Content = JsonContent.Create(dto)
        };

        // Act
        var result = await client.SendRequest<DownloadRequestDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Filename.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task BulkMove_WithValidIds_ReturnsMovedCount()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        await _testSetup.AddEvent(EventConstants.GetCurrentEvent());
        var testPhotos = PhotoConstants.GetTestPhotos(1);
        var testGallery = GalleryConstants.GetTestGallery(2);
        await _testSetup.AddPhotos(testPhotos);
        await _testSetup.AddGallery(testGallery);
        var dto = new BulkPhotoMoveDto
        {
            PhotoIds = testPhotos.Select(p => p.Id).ToList(),
            TargetGalleryId = testGallery.Id
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/image/bulk-move")
        {
            Content = JsonContent.Create(dto)
        };

        // Act
        var result = await client.SendRequest<int>(request);

        // Assert
        result.Should().Be(testPhotos.Count);
    }

    [Fact]
    public async Task BulkMove_WithInvalidGalleryId_ReturnsNotFound()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        var testPhotos = PhotoConstants.GetTestPhotos(1);
        await _testSetup.AddPhotos(testPhotos);
        var dto = new BulkPhotoMoveDto
        {
            PhotoIds = testPhotos.Select(p => p.Id).ToList(),
            TargetGalleryId = 999
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/image/bulk-move")
        {
            Content = JsonContent.Create(dto)
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadImages_WithValidData_ReturnsUploadBatch()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var dto = new UploadMessageDto
        {
            EventId = testEvent.Id,
            PhotoFilenames = new List<string> { "photo1.jpg", "photo2.jpg" }
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/image/upload")
        {
            Content = JsonContent.Create(dto)
        };

        // Act
        var result = await client.SendRequest<UploadBatchDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.PhotoCount.Should().Be(dto.PhotoFilenames.Count);
    }

    [Fact]
    public async Task UploadImages_WithInvalidEventId_ReturnsBadRequest()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        var dto = new UploadMessageDto
        {
            EventId = -1,
            PhotoFilenames = new List<string> { "photo1.jpg", "photo2.jpg" }
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/image/upload")
        {
            Content = JsonContent.Create(dto)
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.BadRequest);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadImages_WithNonExistentEvent_ReturnsNotFound()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        var dto = new UploadMessageDto
        {
            EventId = 123,
            PhotoFilenames = new List<string> { "photo1.jpg", "photo2.jpg" }
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/image/upload")
        {
            Content = JsonContent.Create(dto)
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBatch_WithValidId_ReturnsBatch()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        await _testSetup.AddEvent(EventConstants.GetCurrentEvent());
        var testBatch = PhotoConstants.GetTestUploadBatch();
        await _testSetup.AddUploadBatch(testBatch);
        var photos = PhotoConstants.GetTestPhotos(1);
        foreach (var eventPhoto in photos) eventPhoto.UploadBatchId = 1;
        await _testSetup.AddPhotos(photos);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/image/batch/{testBatch.Id}");

        // Act
        var result = await client.SendRequest<UploadBatchDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testBatch.Id);
        result.PhotoCount.Should().Be(6);
        result.Ready.Should().Be(testBatch.IsReady);
    }

    [Fact]
    public async Task GetBatch_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/image/batch/999");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReadOnlySasToken_ReturnsToken()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/image/sas");

        // Act
        var result = await client.SendRequest<SasUriResponseDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.SasUri.Should().NotBeNullOrEmpty();
        result.ExpiresOn.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task GetUploadSasUri_WithValidEventId_ReturnsSasUri()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/image/sas/{testEvent.Id}");

        // Act
        var result = await client.SendRequest<SasUriResponseDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(testEvent.Id);
    }

    [Fact]
    public async Task GetUploadSasUri_WithInvalidEventId_ReturnsNotFound()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/image/sas/999");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchEventPhotos_WithValidParams_ReturnsResults()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        await _testSetup.AddUser(UserConstants.GetTestEventAdmin());
        await _testSetup.AddEvent(EventConstants.GetCurrentEvent());
        await _testSetup.AddPhotos(PhotoConstants.GetTestPhotos(1));
        var keyOffset = $"{DateTime.UtcNow.AddDays(-1):O}|0";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/image/search?galleryId=1&pageSize=2&keyOffset={keyOffset}");

        // Act
        var result = await client.SendRequest<PagedData<string, EventPhotoListDto>>(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeEmpty();
        result.HasNextPage.Should().BeTrue();
        result.PageSize.Should().Be(2);
        result.KeyOffset.Should().Be(keyOffset);
    }

    [Fact]
    public async Task GetArchiveDownloadById_WithValidId_ReturnsDownloadRequest()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        var testDownloadRequest = PhotoConstants.GetTestDownloadRequest();
        await _testSetup.AddDownloadRequest(testDownloadRequest);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/image/archive-download/{testDownloadRequest.Id}");

        // Act
        var result = await client.SendRequest<DownloadRequestDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testDownloadRequest.Id);
        result.Filename.Should().Be(testDownloadRequest.Filename);
        result.IsReady.Should().Be(testDownloadRequest.IsReady);
        result.ProcessedOn.Should().Be(testDownloadRequest.ProcessedOn);
    }

    [Fact]
    public async Task GetArchiveDownloadById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestPhotographer());
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/image/archive-download/999");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    private EventFotoContext GetDbContext()
    {
        return _scope.ServiceProvider.GetRequiredService<EventFotoContext>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
