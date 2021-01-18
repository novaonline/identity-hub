using CommandLine;

namespace IdentityServerClient.CLI.Options
{
	public class CommonOptions
	{
		[Option('d', "db", Required = true, HelpText = "The Database Connection String")]
		public string DatabaseConnection { get; set; }
	}
}
