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

namespace IdentityServer
{
	public class Startup
	{
		public Startup(IHostingEnvironment env, ILoggerFactory logger)
		{
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

			builder.AddEnvironmentVariables();
			Configuration = builder.Build();
			HostingEnvironment = env;
			Logger = logger.CreateLogger("start up");
		}

		public IConfigurationRoot Configuration { get; }
		public IHostingEnvironment HostingEnvironment { get; }
		public ILogger Logger { get; }


		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// get the assembly 
			var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
			var defaultConnectionString = Configuration.GetConnectionString("DefaultConnection");
			// Add framework services.
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(defaultConnectionString,
				b => b.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name)));

			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();
			// sub comes from https://github.com/IdentityServer/IdentityServer3.Samples/issues/339

			// globally implications
			services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add(new GlobalExceptionFilter(Logger));
				options.Filters.Add(new RequireHttpsAttribute());
				var policy = new AuthorizationPolicyBuilder()
					.RequireAuthenticatedUser()
					.Build();
				options.Filters.Add(new AuthorizeFilter(policy));
			});

			services.Configure<IdentityOptions>(options =>
			{
				//options.Cookies.ApplicationCookie.LoginPath = new PathString("/login");
				//options.Cookies.ApplicationCookie.LogoutPath = new PathString("/logout");
			});

			// Add application services.
			services.AddTransient<IEmailSender, AuthMessageSender>();
			services.AddTransient<ISmsSender, AuthMessageSender>();


			var mvcBuilder = services.AddMvc(config =>
			{
				// globally authorize each controller just in case an authorize decor was missed

			});

			//mvcBuilder.AddMvcOptions(o => { o.Filters.Add(new GlobalExceptionFilter(Logger)); });

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
					options.EnableTokenCleanup = true;
					options.TokenCleanupInterval = 30;
				}).AddAspNetIdentity<ApplicationUser>();

			if (HostingEnvironment.IsProduction())
			{
				// create the signing cred
				identityConfig.AddDeveloperSigningCredential();
			}
			else
			{
				identityConfig.AddDeveloperSigningCredential();
			}
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			// things for debugging
			//loggerFactory.AddConsole(Configuration.GetSection("Logging"));
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

			// this will do the initial DB population
			if (env.IsDevelopment())
			{
				InitializeDatabase(app);
			}

			// security
			app.UseRewriter(new RewriteOptions().AddRedirectToHttps());

			//app.UseAuthentication();

			// Adds IdentityServer
			app.UseIdentityServer();

			// Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}

		private void InitializeDatabase(IApplicationBuilder app)
		{
			// https://stackoverflow.com/questions/45574821/identityserver4-persistedgrantdbcontext-configurationdbcontext
			using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
			{
				serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
				serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

				var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
				context.Database.Migrate();
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

	public class GlobalExceptionFilter : IExceptionFilter
	{
		private readonly ILogger _logger;

		public GlobalExceptionFilter(ILogger logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			this._logger = logger;
		}

		public void OnException(ExceptionContext context)
		{
			this._logger.LogError("GlobalExceptionFilter", context.Exception);
		}
	}
}
