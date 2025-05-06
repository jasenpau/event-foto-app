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

public class GalleryControllerTests : IClassFixture<TestApplicationFactory>, IDisposable
{
    private readonly TestApplicationFactory _factory;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly TestDataSetup _testSetup;
    private readonly IServiceScope _scope;

    public GalleryControllerTests(TestApplicationFactory factory,
        ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
        _testSetup = new TestDataSetup(factory);
        _scope = _factory.Services.CreateScope();
    }

    [Fact]
    public async Task CreateGallery_WithValidData_ReturnsNewGallery()
    {
        // Arrange
        var context = GetDbContext();
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var createDto = new CreateEditGalleryRequestDto
        {
            Name = "New Gallery",
            WatermarkId = null
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/gallery/event/{testEvent.Id}")
        {
            Content = JsonContent.Create(createDto)
        };

        // Act
        var result = await client.SendRequest<GalleryDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        context.Galleries.Count().Should().Be(2);
    }

    [Fact]
    public async Task CreateGallery_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var createDto = new CreateEditGalleryRequestDto
        {
            Name = testEvent.Galleries[0].Name,
            WatermarkId = null
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/gallery/event/{testEvent.Id}")
        {
            Content = JsonContent.Create(createDto)
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.Conflict);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((int)HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateGallery_ForNonExistentEvent_ReturnsNotFound()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var createDto = new CreateEditGalleryRequestDto
        {
            Name = "New gallery",
            WatermarkId = null
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/gallery/event/{123}")
        {
            Content = JsonContent.Create(createDto)
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGalleryAsync_ReturnsGallery()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        var gallery = testEvent.Galleries[0];
        await _testSetup.AddEvent(testEvent);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/gallery/{gallery.Id}");

        // Act
        var result = await client.SendRequest<GalleryDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(gallery.Id);
        result.Name.Should().Be(gallery.Name);
    }

    [Fact]
    public async Task GetGalleryAsync_WithNonExistentGallery_ReturnsNotFound()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/gallery/123");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateGalleryAsync_WithValidData_ReturnsUpdatedGallery()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        var gallery = testEvent.Galleries[0];
        await _testSetup.AddEvent(testEvent);
        var updateDto = new CreateEditGalleryRequestDto
        {
            Name = "Updated Gallery Name",
            WatermarkId = gallery.WatermarkId,
            ReprocessPhotos = true
        };
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/gallery/{gallery.Id}")
        {
            Content = JsonContent.Create(updateDto)
        };

        // Act
        var result = await client.SendRequest<GalleryDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task UpdateGalleryAsync_WithNonExistentGallery_ReturnsNotFound()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var updateDto = new CreateEditGalleryRequestDto
        {
            Name = "Updated Gallery Name",
            WatermarkId = null
        };
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/gallery/123")
        {
            Content = JsonContent.Create(updateDto)
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateGalleryAsync_WithExistingName_ReturnsConflict()
    {
        // Arrange
        var context = GetDbContext();
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var newGallery = GalleryConstants.GetTestGallery(2);
        await _testSetup.AddGallery(newGallery);
        var updateDto = new CreateEditGalleryRequestDto
        {
            Name = testEvent.Galleries[0].Name,
            WatermarkId = null
        };
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/gallery/{newGallery.Id}")
        {
            Content = JsonContent.Create(updateDto)
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.Conflict);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((int)HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteGallery_WithValidId_ReturnsTrue()
    {
        // Arrange
        var context = GetDbContext();
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var newGallery = GalleryConstants.GetTestGallery(2);
        await _testSetup.AddGallery(newGallery);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/gallery/{newGallery.Id}");

        // Act
        var result = await client.SendRequest<bool>(request);

        // Assert
        result.Should().BeTrue();
        context.Galleries.Count().Should().Be(1);
    }

    [Fact]
    public async Task DeleteGallery_WithNonExistentGallery_ReturnsNotFound()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/gallery/123");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteGallery_WhenDefaultGallery_ReturnsBadRequest()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        var gallery = testEvent.Galleries[0];
        await _testSetup.AddEvent(testEvent);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/gallery/{gallery.Id}");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.BadRequest);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((int)HttpStatusCode.BadRequest);
        result.Title.Should().Be("default-gallery");
    }

    [Fact]
    public async Task DeleteGallery_WhenHasPhotos_ReturnsBadRequest()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        var gallery = testEvent.Galleries[0];
        await _testSetup.AddEvent(testEvent);
        var newGallery = GalleryConstants.GetTestGallery(2);
        await _testSetup.AddGallery(newGallery);
        await _testSetup.AddPhotos(PhotoConstants.GetTestPhotos(newGallery.Id));
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/gallery/{newGallery.Id}");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.BadRequest);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((int)HttpStatusCode.BadRequest);
        result.Title.Should().Be("not-empty-gallery");
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
