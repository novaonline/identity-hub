using IdentityServ.Rules;
using IdentityServer.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IdentityServer.Helpers.Migration
{
	public static class MigrationHelper
	{

		public static void InitializeDatabase(this IApplicationBuilder app, IWebHostEnvironment env)
		{
			// https://stackoverflow.com/questions/45574821/identityserver4-persistedgrantdbcontext-configurationdbcontext
			using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
			{
				serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
				serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

				var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
				context.Database.EnsureCreated();
				if (context.Database.GetPendingMigrations().Any())
				{
					context.Database.Migrate();
				}
				if (env.IsDevelopment())
				{
					// Seed data if none exists
					if (!context.Clients.Any())
					{
						context.Clients.RemoveRange(context.Clients.AsEnumerable());
						foreach (var client in Config.GetClients())
						{
							context.Clients.Add(client.ToEntity());
						}
						context.SaveChanges();
					}

					if (!context.IdentityResources.Any())
					{
						context.IdentityResources.RemoveRange(context.IdentityResources.AsEnumerable());
						foreach (var resource in Config.GetIdentityResources())
						{
							context.IdentityResources.Add(resource.ToEntity());
						}
						context.SaveChanges();
					}

					if (!context.ApiResources.Any())
					{
						context.ApiResources.RemoveRange(context.ApiResources.AsEnumerable());
						foreach (var resource in Config.GetApiResources())
						{
							context.ApiResources.Add(resource.ToEntity());
						}
						context.SaveChanges();
					}
				}
			}
		}

		public static string GetMigrationAssembly() => typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

		public static void UseDefault(this DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("DefaultConnection");
			// We add Assembly here because we added the context in another project.
			optionsBuilder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(GetMigrationAssembly()));
		}
	}
}
