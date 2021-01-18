using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;

namespace IdentityServer
{
	public class Program
	{
		public static void Main(string[] args) => BuildWebHost(args).Run();

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
			.CaptureStartupErrors(true)
			.UseSetting(WebHostDefaults.DetailedErrorsKey, "true")
			.UseStartup<Startup>()
			.Build();
	}
}
