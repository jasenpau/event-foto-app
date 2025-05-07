using EventFoto.Processor.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace EventFoto.Tests.FunctionTests;

public class CleanupFunctionTests
{
    private readonly FunctionTestHost _host;
    private readonly CleanupFunction _function;
    private readonly FunctionContext _functionContext;

    public CleanupFunctionTests()
    {
        _host = new FunctionTestHost(services =>
        {
            services.AddTransient<CleanupFunction>();
        });
        _function = _host.GetService<CleanupFunction>();
        _functionContext = _host.GetService<FunctionContext>();
    }

    [Fact]
    public async Task CleanupFunction_Runs()
    {
        // Arrange
        var timer = new TimerInfo();

        // Act
        await _function.RunAsync(timer, _functionContext, CancellationToken.None);

        // Assert
        // Add your assertions here based on the expected behavior of the CleanupFunction
    }
}
