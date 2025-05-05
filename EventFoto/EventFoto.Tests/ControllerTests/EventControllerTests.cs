using System.Net;
using System.Net.Http.Json;
using EventFoto.Data;
using EventFoto.Data.DTOs;
using EventFoto.Tests.TestBedSetup;
using EventFoto.Tests.TestConstants;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EventFoto.Tests.ControllerTests;

public class EventControllerTests : IClassFixture<TestApplicationFactory>, IDisposable
{
    private readonly TestApplicationFactory _factory;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly TestDataSetup _testSetup;
    private readonly IServiceScope _scope;

    public EventControllerTests(TestApplicationFactory factory,
        ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
        _testSetup = new TestDataSetup(factory);
        _scope = _factory.Services.CreateScope();
    }

    [Fact]
    public async Task GetEventById_ReturnsEvent()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/event/{testEvent.Id}");

        // Act
        var result = await client.SendRequest<EventDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testEvent.Id);
        result.Name.Should().Be(testEvent.Name);
    }

    [Fact]
    public async Task CreateEvent_WithValidData_ReturnsNewEvent()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var createDto = new CreateEditEventDto
        {
            Name = "New Event",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/event")
        {
            Content = JsonContent.Create(createDto)
        };

        // Act
        var result = await client.SendRequest<EventDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
    }

    [Fact]
    public async Task UpdateEvent_WithValidData_ReturnsUpdatedEvent()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var updateDto = new CreateEditEventDto
        {
            Name = "Updated Event Name",
            StartDate = testEvent.StartDate,
            EndDate = testEvent.EndDate,
        };
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/event/{testEvent.Id}")
        {
            Content = JsonContent.Create(updateDto)
        };

        // Act
        var result = await client.SendRequest<EventDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testEvent.Id);
        result.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task UpdateEvent_WithReprocessData_AddsProcessingToQueue()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var updateDto = new CreateEditEventDto
        {
            Name = "Updated Event Name",
            StartDate = testEvent.StartDate,
            EndDate = testEvent.EndDate,
            WatermarkId = testEvent.WatermarkId,
            ReprocessPhotos = true,
        };
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/event/{testEvent.Id}")
        {
            Content = JsonContent.Create(updateDto)
        };

        // Act
        var result = await client.SendRequest<EventDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testEvent.Id);
        result.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task UpdateEvent_WithNonExistentEvent_ReturnsNotFound()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var updateDto = new CreateEditEventDto
        {
            Name = "Updated Event Name",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
        };
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/event/{123}")
        {
            Content = JsonContent.Create(updateDto)
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEvent_WithArchivedEvent_ReturnsTrue()
    {
        // Arrange
        var context = GetDbContext();
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        testEvent.IsArchived = true;
        testEvent.ArchiveName = "test-archive.zip";
        await _testSetup.AddEvent(testEvent);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/event/{testEvent.Id}");

        // Act
        var result = await client.SendRequest<bool>(request);

        // Assert
        result.Should().BeTrue();
        context.Events.Count().Should().Be(0);
    }

    [Fact]
    public async Task DeleteEvent_WithNotArchivedEvent_ReturnsBadRequest()
    {
        // Arrange
        var context = GetDbContext();
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/event/{testEvent.Id}");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.BadRequest);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.BadRequest);
        context.Events.Count().Should().Be(1);
    }

    [Fact]
    public async Task DeleteEvent_WithNonExistentEvent_ReturnsNotFound()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/event/{123}");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEventPhotographers_ReturnsPhotographers()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/event/{testEvent.Id}/photographers");

        // Act
        var result = await client.SendRequest<IList<AssignmentResponseDto>>(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEventPhotographers_NonExistentEvent_ReturnsNotFound()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/event/{123}/photographers");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCurrentPhotographerAssignment_WithValidData_ReturnsAssignment()
    {
        // Arrange
        var testUser = UserConstants.GetTestPhotographer();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        await _testSetup.AssignPhotographer(testEvent.Galleries[0].Id, testUser.Id);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/event/{testEvent.Id}/photographers/current");

        // Act
        var result = await client.SendRequest<AssignmentResponseDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.GalleryId.Should().Be(testEvent.Galleries[0].Id);
        result.UserId.Should().Be(testUser.Id);
        result.GalleryName.Should().Be(testEvent.Galleries[0].Name);
        result.UserName.Should().Be(testUser.Name);
    }

    [Fact]
    public async Task GetCurrentPhotographerAssignment_WithNoAssignment_ReturnsNotAssigned()
    {
        // Arrange
        var testUser = UserConstants.GetTestPhotographer();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/event/{testEvent.Id}/photographers/current");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((int)HttpStatusCode.NotFound);
        result.Title.Should().Be("not-assigned");
    }

    [Fact]
    public async Task AssignPhotographer_WithValidData_ReturnsTrue()
    {
        // Arrange
        var context = GetDbContext();
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testPhotographer = UserConstants.GetTestPhotographer();
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        await _testSetup.AddUser(testPhotographer);
        await _testSetup.AssignPhotographer(testEvent.Galleries[0].Id, testPhotographer.Id);
        var assignDto = new AssignmentRequestDto
        {
            GalleryId = testEvent.Galleries[0].Id,
            UserId = testPhotographer.Id,
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/event/{testEvent.Id}/photographers")
        {
            Content = JsonContent.Create(assignDto)
        };

        // Act
        var result = await client.SendRequest<bool>(request);

        // Assert
        result.Should().BeTrue();
        var createdAssignment = context.PhotographerAssignments.FirstOrDefault();
        createdAssignment.Should().NotBeNull();
        createdAssignment.GalleryId.Should().Be(assignDto.GalleryId);
        createdAssignment.UserId.Should().Be(assignDto.UserId);
    }
    
    [Fact]
    public async Task AssignPhotographer_WhenPhotographerAssignsOthers_ReturnsUnauthorized()
    {
        // Arrange
        var context = GetDbContext();
        var testPhotographer = UserConstants.GetTestPhotographer();
        var testAdmin = UserConstants.GetTestEventAdmin();
        var testEvent = EventConstants.GetCurrentEvent();
        var client = await _testSetup.SetupWithUser(testPhotographer);
        await _testSetup.AddUser(testAdmin);
        await _testSetup.AddEvent(testEvent);

        var assignDto = new AssignmentRequestDto
        {
            GalleryId = testEvent.Galleries[0].Id,
            UserId = testAdmin.Id,
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/event/{testEvent.Id}/photographers")
        {
            Content = JsonContent.Create(assignDto)
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.Unauthorized);

        // Assert
        result?.Status.Should().Be((int)HttpStatusCode.Unauthorized);
        context.PhotographerAssignments.Count().Should().Be(0);
    }

    [Fact]
    public async Task AssignPhotographer_WhenEventDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var context = GetDbContext();
        var testAdmin = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testAdmin);

        var assignDto = new AssignmentRequestDto
        {
            GalleryId = 123,
            UserId = testAdmin.Id,
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/event/{231}/photographers")
        {
            Content = JsonContent.Create(assignDto)
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result?.Status.Should().Be((int)HttpStatusCode.NotFound);
        context.PhotographerAssignments.Count().Should().Be(0);
    }

    [Fact]
    public async Task AssignPhotographer_WhenUserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var context = GetDbContext();
        var testAdmin = UserConstants.GetTestEventAdmin();
        var testEvent = EventConstants.GetCurrentEvent();

        var client = await _testSetup.SetupWithUser(testAdmin);
        await _testSetup.AddEvent(testEvent);

        var assignDto = new AssignmentRequestDto
        {
            GalleryId = testEvent.Galleries[0].Id,
            UserId = Guid.Parse("12345678-1234-1234-1234-123456789012")
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/event/{testEvent.Galleries[0].Id}/photographers")
        {
            Content = JsonContent.Create(assignDto)
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result?.Status.Should().Be((int)HttpStatusCode.NotFound);
        context.PhotographerAssignments.Count().Should().Be(0);
    }

    [Fact]
    public async Task UnassignPhotographer_WithValidData_ReturnsTrue()
    {
        // Arrange
        var testAdmin = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testAdmin);
        var testPhotographer = UserConstants.GetTestPhotographer();
        await _testSetup.AddUser(testPhotographer);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        await _testSetup.AssignPhotographer(testEvent.Galleries[0].Id, testPhotographer.Id);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/event/{testEvent.Id}/photographers/{testPhotographer.Id}");

        // Act
        var result = await client.SendRequest<bool>(request);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UnassignPhotographer_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var testAdmin = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testAdmin);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/event/{testEvent.Id}/photographers/{Guid.NewGuid()}");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnassignPhotographer_WithNonExistentEvent_ReturnsNotFound()
    {
        // Arrange
        var testAdmin = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testAdmin);
        var testPhotographer = UserConstants.GetTestPhotographer();
        await _testSetup.AddUser(testPhotographer);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/event/{123}/photographers/{testPhotographer.Id}");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnassignPhotographer_WhenPhotographerUnassignsOthers_ReturnsUnauthorized()
    {
        // Arrange
        var context = GetDbContext();
        var testPhotographer = UserConstants.GetTestPhotographer();
        var testAdmin = UserConstants.GetTestEventAdmin();
        var testEvent = EventConstants.GetCurrentEvent();
        var client = await _testSetup.SetupWithUser(testPhotographer);
        await _testSetup.AddUser(testAdmin);
        await _testSetup.AddEvent(testEvent);
        await _testSetup.AssignPhotographer(testEvent.Galleries[0].Id, testAdmin.Id);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/event/{testEvent.Id}/photographers/{testAdmin.Id}");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.Unauthorized);

        // Assert
        result?.Status.Should().Be((int)HttpStatusCode.Unauthorized);
        context.PhotographerAssignments.Count().Should().Be(1);
    }

    [Fact]
    public async Task ArchiveEvent_WithValidId_ReturnsArchiveName()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var testEvent = EventConstants.GetCurrentEvent();
        await _testSetup.AddEvent(testEvent);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/event/{testEvent.Id}/archive");

        // Act
        var result = await client.SendRequest<ArchiveResponseDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.ArchiveName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ArchiveEvent_WithNonExistentEvent_ReturnsNotFound()
    {
        // Arrange
        var testUser = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testUser);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/event/{123}/archive");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCalendarFile_ReturnsCalendarFile()
    {
        // Arrange
        var client = await _testSetup.SetupEmpty();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/event/calendar");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/calendar");
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
