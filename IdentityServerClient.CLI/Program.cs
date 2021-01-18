using IdentityServerClient.CLI.Options;
using CommandLine;
using IdentityServerClient.CLI.Service;

namespace IdentityServerClient.CLI
{
	class Program
	{
		/// <summary>
		/// Quickly add an admin when starting application for first time
		/// </summary>
		/// <param name="args"></param>
		static int Main(string[] args)
		{
			return Parser.Default.ParseArguments<AddAdminOptions>(args).MapResult(
				(AddAdminOptions opts) => AdminService.AddAsync(opts).Result,
				errs => 1);
		}
	}
}
