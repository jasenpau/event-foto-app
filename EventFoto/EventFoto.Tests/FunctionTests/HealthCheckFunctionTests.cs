using System.Net;
using EventFoto.Processor.Functions;
using EventFoto.Tests.TestBedSetup;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace EventFoto.Tests.FunctionTests;

public class HealthCheckFunctionTests
{
    private readonly FunctionTestHost _host;
    private readonly HealthCheckFunction _function;
    private readonly FunctionContext _functionContext;

    public HealthCheckFunctionTests()
    {
        _host = new FunctionTestHost(services =>
        {
            services.AddTransient<HealthCheckFunction>();
        });
        _function = _host.GetService<HealthCheckFunction>();
        _functionContext = _host.GetService<FunctionContext>();
    }

    [Fact]
    public async Task HealthCheckFunction_ReturnsOk()
    {
        // Arrange
        var httpRequest = new MockHttpRequestData(_functionContext);

        // Act
        var result = await _function.Run(httpRequest, _functionContext);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Body.Position = 0;
        using var reader = new StreamReader(result.Body);
        var response = await reader.ReadToEndAsync();
        response.Should().Be("Healthy");
    }
}
