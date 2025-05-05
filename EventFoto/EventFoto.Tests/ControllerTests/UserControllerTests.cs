using System.Net;
using System.Net.Http.Json;
using EventFoto.Data;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using EventFoto.Tests.TestBedSetup;
using EventFoto.Tests.TestConstants;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EventFoto.Tests.ControllerTests;

public class UserControllerTests : IClassFixture<TestApplicationFactory>, IDisposable
{
    private readonly TestApplicationFactory _factory;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly TestDataSetup _testSetup;
    private readonly IServiceScope _scope;

    public UserControllerTests(TestApplicationFactory factory,
        ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
        _testSetup = new TestDataSetup(factory);
        _scope = _factory.Services.CreateScope();
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsCurrentUser()
    {
        // Arrange
        var testUser = UserConstants.GetTestViewer();
        var client = await _testSetup.SetupWithUser(testUser);
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/user/current");

        // Act
        var result = await client.SendRequest<UserDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testUser.Id);
        result.Name.Should().Be(testUser.Name);
        result.Email.Should().Be(testUser.Email);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsNewUser()
    {
        // Arrange
        var testUser = UserConstants.GetTestPhotographer();
        var client = await _testSetup.SetupWithUser(testUser);
        var registerDto = new RegisterDto()
        {
            Name = "New User Name",
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/user/register")
        {
            Content = JsonContent.Create(registerDto)
        };

        // Act
        var result = await client.SendRequest<UserDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testUser.Id);
        result.Name.Should().Be(registerDto.Name);
    }

    [Fact]
    public async Task SearchUsers_ReturnsPagedResults()
    {
        // Arrange
        var testUser = UserConstants.GetTestPhotographer();
        var client = await _testSetup.SetupWithUser(testUser);
        await _testSetup.AddUser(UserConstants.GetTestEventAdmin());
        await _testSetup.AddUser(UserConstants.GetTestSystemAdmin());
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/user/search?pageSize=10&keyOffset=Test%20Event%20Admin");

        // Act
        var result = await client.SendRequest<PagedData<string, UserListDto>>(request);

        // Assert
        result.Should().NotBeNull();
        result.HasNextPage.Should().BeFalse();
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchUsers_WithAssignedEvent_ReturnsExcludedUsers()
    {
        // Arrange
        var testUser = UserConstants.GetTestPhotographer();
        var client = await _testSetup.SetupWithUser(testUser);
        await _testSetup.AddUser(UserConstants.GetTestEventAdmin());
        await _testSetup.AddUser(UserConstants.GetTestSystemAdmin());
        var testEvent = EventConstants.GetCurrentEvent();
        testEvent.Galleries[0].Assignments = new List<PhotographerAssignment>
        {
            new()
            {
                UserId = testUser.Id,
                GalleryId = testEvent.Galleries[0].Id,
            }
        };
        await _testSetup.AddEvent(testEvent);
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/user/search?pageSize=10&excludeEventId=1");

        // Act
        var result = await client.SendRequest<PagedData<string, UserListDto>>(request);

        // Assert
        result.Should().NotBeNull();
        result.HasNextPage.Should().BeFalse();
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task InviteUser_WithValidData_ReturnsInvitedUserId()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
        var inviteDto = new UserInviteRequestDto
        {
            Email = "invite@example.com",
            Name = "Invited user",
            GroupAssignment = UserConstants.PhotographerGroupId.ToString(),
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/user/invite")
        {
            Content = JsonContent.Create(inviteDto)
        };

        // Act
        var result = await client.SendRequest<Guid>(request);

        // Assert
        result.Should().Be(UserConstants.GetMockInvitationResponse().InvitedUser?.Id);
    }

    [Fact]
    public async Task InviteUser_WithValidData_CreatesUser()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
        var inviteDto = new UserInviteRequestDto
        {
            Email = "invite@example.com",
            Name = "Invited user",
            GroupAssignment = UserConstants.PhotographerGroupId.ToString(),
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/user/invite")
        {
            Content = JsonContent.Create(inviteDto)
        };

        // Act
        var result = await client.SendRequest<Guid>(request);

        // Assert
        var context = GetDbContext();
        var user = await context.Users.FindAsync(result);
        user.Should().NotBeNull();
        user.Name.Should().Be(inviteDto.Name);
        user.Email.Should().Be(inviteDto.Email);
        user.GroupAssignment.Should().Be(UserConstants.PhotographerGroupId);
    }

    [Fact]
    public async Task InviteUser_WithInvalidGroup_ReturnsBadRequest()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
        var inviteDto = new UserInviteRequestDto
        {
            Email = "test@example.com",
            Name = "Test User",
            GroupAssignment = "invalid-group"
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/user/invite")
        {
            Content = JsonContent.Create(inviteDto)
        };

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InviteUser_WithExistingEmail_ReturnsConflict()
    {
        // Arrange
        var testAdmin = UserConstants.GetTestEventAdmin();
        var client = await _testSetup.SetupWithUser(testAdmin);
        var inviteDto = new UserInviteRequestDto
        {
            Email = testAdmin.Email,
            Name = "Test User",
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/user/invite")
        {
            Content = JsonContent.Create(inviteDto)
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.Conflict);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((int)HttpStatusCode.Conflict);
        result.Title.Should().Be("exists");
    }

    [Fact]
    public async Task ValidateInvite_WithValidKey_ReturnsTrue()
    {
        // Arrange
        var client = await _testSetup.SetupEmpty();
        var invitedUser = UserConstants.GetInvitedUser();
        await _testSetup.AddUser(invitedUser);
        var validateDto = new InviteValidateRequestDto { InviteKey = invitedUser.InvitationKey };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/user/validate-invite")
        {
            Content = JsonContent.Create(validateDto)
        };

        // Act
        var result = await client.SendRequest<bool>(request);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateInvite_WithInvalidKey_ReturnsFalse()
    {
        // Arrange
        var client = await _testSetup.SetupEmpty();
        var validateDto = new InviteValidateRequestDto { InviteKey = "invalid-key" };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/user/validate-invite")
        {
            Content = JsonContent.Create(validateDto)
        };

        // Act
        var result = await client.SendRequest<bool>(request);

        // Assert
        Assert.False(result);
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
