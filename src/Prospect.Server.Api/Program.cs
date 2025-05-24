using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Prospect.Server.Api;

public static class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = CreateLogger();

        try
        {
            Log.Information("Starting web host");
            CreateHostBuilder(args).Build().Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static Logger CreateLogger()
    {
        const string OutputTemplate = "[{Timestamp:HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} | {Properties:lj} {NewLine}{Exception}";

        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(s => s.Console(outputTemplate: OutputTemplate))
            .WriteTo.Async(s => s.File(
                path: "./logs/.txt",
                outputTemplate: OutputTemplate,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 3,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 100 * 1024 * 1024 // 100MB
            ))
            .CreateLogger();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
