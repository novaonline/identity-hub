using IdentityServerClient.CLI.Options;
using CommandLine;
using System;
using IdentityServerClient.CLI.Service;

namespace IdentityServerClient.CLI
{
	class Program
	{
		/// <summary>
		/// Quickly create, remove and update clients
		/// </summary>
		/// <param name="args"></param>
		static int Main(string[] args)
		{
			return Parser.Default.ParseArguments<AddOptions, RemoveOptions, UpdateOptions>(args).MapResult(
				(AddOptions opts) => ClientService.Add(opts),
				(RemoveOptions opts) => ClientService.Remove(opts),
				(UpdateOptions opts) => ClientService.Update(opts),
				errs => 1);
		}
	}
}
