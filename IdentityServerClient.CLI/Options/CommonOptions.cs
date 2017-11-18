﻿using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServerClient.CLI.Options
{
	public class CommonOptions
	{
		[Option('n', "name", Required = true, HelpText = "Name of the client")]
		public string Name { get; set; }

		[Option('d', "db", HelpText = "The Database Connection. Defaulted to a local instance")]
		public string DatabaseConnection { get; set; }
	}
}