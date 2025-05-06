using System.Net;
using System.Net.Http.Headers;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using EventFoto.Tests.TestBedSetup;
using EventFoto.Tests.TestConstants;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EventFoto.Tests.ControllerTests;

public class WatermarkControllerTests : IClassFixture<TestApplicationFactory>, IDisposable
{
    private readonly TestApplicationFactory _factory;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly TestDataSetup _testSetup;
    private readonly IServiceScope _scope;

    public WatermarkControllerTests(TestApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
        _testSetup = new TestDataSetup(factory);
        _scope = _factory.Services.CreateScope();
    }

    [Fact]
    public async Task UploadWatermark_WithValidData_ReturnsWatermark()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var formData = new MultipartFormDataContent
        {
            { fileContent, "file", "test-watermark.png" },
            { new StringContent("Test Watermark"), "name" }
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/watermark")
        {
            Content = formData
        };

        // Act
        var result = await client.SendRequest<WatermarkDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Watermark");
    }

    [Fact]
    public async Task UploadWatermark_WithNoFile_ReturnsBadRequest()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
        var formData = new MultipartFormDataContent
        {
            { new StringContent("Test Watermark"), "name" }
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/watermark")
        {
            Content = formData
        };

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.BadRequest);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteWatermark_WithValidId_ReturnsTrue()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
        var testWatermark = WatermarkConstants.GetTestWatermark();
        await _testSetup.AddWatermark(testWatermark);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/watermark/{testWatermark.Id}");

        // Act
        var result = await client.SendRequest<bool>(request);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteWatermark_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/watermark/999");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetWatermark_WithValidId_ReturnsWatermark()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
        var testWatermark = WatermarkConstants.GetTestWatermark();
        await _testSetup.AddWatermark(testWatermark);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/watermark/{testWatermark.Id}");

        // Act
        var result = await client.SendRequest<WatermarkDto>(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testWatermark.Id);
        result.Name.Should().Be(testWatermark.Name);
    }

    [Fact]
    public async Task GetWatermark_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/watermark/999");

        // Act
        var result = await client.SendRequest<ProblemDetails>(request, HttpStatusCode.NotFound);

        // Assert
        result!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchWatermarks_WithValidQuery_ReturnsResults()
    {
        // Arrange
        var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
        await _testSetup.AddWatermarks(WatermarkConstants.GetTestWatermarks());
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/watermark/search?pageSize=2&keyOffset=2");

        // Act
        var result = await client.SendRequest<PagedData<string, WatermarkDto>>(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeEmpty();
        result.HasNextPage.Should().BeTrue();
        result.PageSize.Should().Be(2);
        result.KeyOffset.Should().Be("2");
    }

    // [Fact]
    // public async Task SearchWatermarks_WithNoResults_ReturnsEmpty()
    // {
    //     // Arrange
    //     var client = await _testSetup.SetupWithUser(UserConstants.GetTestEventAdmin());
    //     var request = new HttpRequestMessage(HttpMethod.Get, "/api/watermark/search");
    //
    //     // Act
    //     var result = await client.SendRequest<PagedData<string, WatermarkDto>>(request);
    //
    //     // Assert
    //     result.Should().NotBeNull();
    //     result.Data.Should().BeEmpty();
    // }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
