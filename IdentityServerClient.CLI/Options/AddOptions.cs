using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServerClient.CLI.Options
{
	[Verb("add", HelpText = "Quickly Add a client")]
	public class AddOptions : CommonOptions
	{
		[Option('u', "url", Required = true, HelpText = "The base url of the website")]
		public string BaseUrl { get; set; }

		//[Option('h', "html", HelpText = "Static pages for redirects. For example callback.html")]
		//public bool IsStaticPagesForRedirect { get; set; }

		[Option('s', "scope", HelpText = "scopes allowed")]
		public IEnumerable<string> Scopes { get; set; }
	}
}
