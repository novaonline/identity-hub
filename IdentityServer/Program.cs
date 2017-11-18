using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace IdentityServer
{
	public class Program
    {
		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
			 .MinimumLevel.Debug()
			 .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
			 .MinimumLevel.Override("System", LogEventLevel.Warning)
			 .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
			 .Enrich.FromLogContext()
			 .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
			 .WriteTo.ApplicationInsightsEvents("65bf1cd7-7799-4619-a51c-a3c07ebb89af")
			 .CreateLogger();

			BuildWebHost(args).Run();

		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.CaptureStartupErrors(true)
				.UseSetting(WebHostDefaults.DetailedErrorsKey, "true")
				.UseStartup<Startup>()
				.UseSerilog()
				.Build();
	}
}
