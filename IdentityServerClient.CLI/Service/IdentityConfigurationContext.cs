using IdentityServer4.EntityFramework.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.Options;

namespace IdentityServerClient.CLI.Service
{
    public class IdentityConfigurationContext : ConfigurationDbContext
	{
		private Options.AddOptions options;
		public IdentityConfigurationContext(
			DbContextOptions<ConfigurationDbContext> options, 
			ConfigurationStoreOptions storeOptions,
			Options.AddOptions addOptions) 
			: base(options, storeOptions)
		{
			this.options = addOptions;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(options.DatabaseConnection ?? "Server=(localdb)\\mssqllocaldb;Database=identityserver-dev;Trusted_Connection=True;MultipleActiveResultSets=true");
		}
	}
}
