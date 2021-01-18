using CommandLine;

namespace IdentityServerClient.CLI.Options
{
	[Verb("add-admin", HelpText = "Quickly add an admin role to either an existing or nonexistent user")]
	public class AddAdminOptions : CommonOptions
	{
		[Option('e', "email", Required = true, HelpText = "The email address for admin user")]
		public string Email { get; set; }
	}
}
