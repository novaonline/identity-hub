using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentityServer.Services;
using IdentityServer.Configuration;
using IdentityServ.Rules;
using IdentityServ.Models;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using IdentityServer.Helpers;

namespace IdentityServer
{
	public class Startup
	{
		public IConfigurationRoot Configuration { get; }
		public IHostingEnvironment HostingEnvironment { get; }
		public ILogger Logger { get; }

		public Startup(IHostingEnvironment env, ILoggerFactory logger)
		{
			// Setting up Configuration 
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();
			if (env.IsDevelopment())
			{
				// For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
				builder.AddUserSecrets<Startup>();
			}
			Configuration = builder.Build();
			HostingEnvironment = env;
			Logger = logger.CreateLogger("Startup");
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// get the assembly 
			var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
			var defaultConnectionString = Configuration.GetConnectionString("DefaultConnection");
			Logger.LogInformation($"Connection string fetched with a length of {defaultConnectionString.Length}");

			// Add framework services.
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(defaultConnectionString,
				b => b.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name)));

			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			// setup global implications (i.e. defaults)
			services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add(new GlobalExceptionFilter(Logger)); // exception handling
				options.Filters.Add(new RequireHttpsAttribute()); // enforce ssl 
				var policy = new AuthorizationPolicyBuilder()
					.RequireAuthenticatedUser()
					.Build();
				options.Filters.Add(new AuthorizeFilter(policy)); // authorize by default
			});

			// Additional DI stuff
			services.AddTransient<IEmailSender, AuthMessageSender>();
			services.AddTransient<ISmsSender, AuthMessageSender>();

			// Adds IdentityServer
			var identityConfig = services.AddIdentityServer(o =>
				{
					o.UserInteraction.LoginUrl = "/login";
					o.UserInteraction.LogoutUrl = "/logout";
				})
				.AddConfigurationStore(options =>
				{
					options.ConfigureDbContext = buider =>
						buider.UseSqlServer(defaultConnectionString, sql =>
						sql.MigrationsAssembly(migrationsAssembly));
				})
				.AddOperationalStore(options =>
				{
					options.ConfigureDbContext = builder =>
						builder.UseSqlServer(defaultConnectionString,
							sql => sql.MigrationsAssembly(migrationsAssembly));

					// this enables automatic token cleanup. this is optional.
					// defaulted to every hour
					options.EnableTokenCleanup = true;
				})
				.AddAspNetIdentity<ApplicationUser>();

			if (HostingEnvironment.IsProduction())
			{
				// create the signing cred
				identityConfig.AddDeveloperSigningCredential();
			}
			else
			{
				identityConfig.AddSigningCredential(IdentityServerBuilderExtensionsCrypto.CreateRsaSecurityKey());
			}
			services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			Logger.LogDebug("Begin Piping");

			// things for debugging
			if (env.IsDevelopment())
			{
				loggerFactory.AddDebug();
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
				app.UseBrowserLink();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}
			
			// rewrites
			app.UseRewriter(new RewriteOptions().AddRedirectToHttps());

			// make sure that a person is authorized
			app.UseAuthentication();

			// use IdentityServer
			app.UseIdentityServer();

			// use static files
			app.UseStaticFiles();

			// finally, use mvc
			app.UseMvcWithDefaultRoute();

			Logger.LogDebug("End Piping");
		}
	}
}
