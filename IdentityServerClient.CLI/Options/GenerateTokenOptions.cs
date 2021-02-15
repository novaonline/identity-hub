using CommandLine;

namespace IdentityServerClient.CLI.Options
{
	[Verb("generate-token", HelpText = "Quickly generate a token")]
	public class GenerateTokenOptions : CommonOptions
	{
		[Option("client-id", Required = true, HelpText = "The client id")]
		public string ClientId { get; set; }

		[Option("client-secret", Required = false, HelpText = "The secret")]
		public string ClientSecret { get; set; }

		[Option("scope", Required = false, HelpText = "Scopes")]
		public string Scope { get; set; }

		[Option("base-url", Required = false, HelpText = "The base url")]
		public string BaseUrl { get; set; }
	}
}
