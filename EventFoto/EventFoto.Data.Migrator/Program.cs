using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventFoto.Data.Migrator;

class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.local.json", optional: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("Database");
                services.AddDbContext<EventFotoContext>(options =>
                    options.UseNpgsql(connectionString));
            })
            .Build();

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventFotoContext>();
        Console.WriteLine("Applying migrations...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Migrations applied.");
    }
}
