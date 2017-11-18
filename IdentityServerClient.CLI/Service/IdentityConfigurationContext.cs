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
		public IdentityConfigurationContext(DbContextOptions<ConfigurationDbContext> options, ConfigurationStoreOptions storeOptions) 
			: base(options, storeOptions)
		{
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=identityserver-dev;Trusted_Connection=True;MultipleActiveResultSets=true");
		}
	}
}
