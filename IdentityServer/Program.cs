using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using System.Net;

namespace IdentityServer
{
	public class Program
	{
		public static void Main(string[] args) => BuildWebHost(args).Run();

		public static IWebHost BuildWebHost(string[] args)
		{
			var hostConfig = new ConfigurationBuilder()
							   .AddEnvironmentVariables()
							   .Build();

			return WebHost.CreateDefaultBuilder(args)
			.CaptureStartupErrors(true)
			.UseSetting(WebHostDefaults.DetailedErrorsKey, "true")
			.UseStartup<Startup>()
			.Build();
		}

	}
}
