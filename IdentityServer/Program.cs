using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Logging.AzureAppServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer
{
	public class Program
	{
		public static void Main(string[] args) => BuildWebHost(args).Run();

		public static IWebHost BuildWebHost(string[] args)
		{
			return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    // We have to be precise on the logging levels
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddAzureWebAppDiagnostics();
                })
                .ConfigureServices(services =>
                {
                    services.Configure<AzureFileLoggerOptions>(options =>
                    {
                        options.FileName = "my-azure-diagnostics-";
                        options.FileSizeLimit = 50 * 1024;
                        options.RetainedFileCountLimit = 5;
                    });
                })
            .CaptureStartupErrors(true)
			.UseSetting(WebHostDefaults.DetailedErrorsKey, "true")
			.UseStartup<Startup>()
			.Build();
		}

	}
}
