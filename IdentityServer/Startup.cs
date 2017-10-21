using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
using Microsoft.AspNetCore.StaticFiles;

namespace IdentityServer
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

			if (env.IsDevelopment())
			{
				// For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
				builder.AddUserSecrets<Startup>();
			}

			builder.AddEnvironmentVariables();
			Configuration = builder.Build();
			HostingEnvironment = env;
		}

		public IConfigurationRoot Configuration { get; }
		public IHostingEnvironment HostingEnvironment { get; }

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

			// globally imply that every controller requires https
			services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add(new RequireHttpsAttribute());
			});

			services.Configure<IdentityOptions>(options =>
			{
				options.Cookies.ApplicationCookie.LoginPath = new PathString("/login");
				options.Cookies.ApplicationCookie.LogoutPath = new PathString("/logout");
			});

			services.AddMvc(config =>
			{
				// globally authorize each controller just in case an authorize decor was missed
				var policy = new AuthorizationPolicyBuilder()
				 .RequireAuthenticatedUser()
				 .Build();
				config.Filters.Add(new AuthorizeFilter(policy));
			});

			// Add application services.
			services.AddTransient<IEmailSender, AuthMessageSender>();
			services.AddTransient<ISmsSender, AuthMessageSender>();

			// Adds IdentityServer
			var identityConfig = services.AddIdentityServer(o =>
				{
					o.UserInteraction.LoginUrl = "/login";
					o.UserInteraction.LogoutUrl = "/logout";
				})
				.AddConfigurationStore(builder =>
				{
					builder.UseSqlServer(defaultConnectionString, options =>
					{
						options.MigrationsAssembly(migrationsAssembly);
					});
				})
				.AddOperationalStore(builder =>
				{
					builder.UseSqlServer(defaultConnectionString, options =>
					{
						options.MigrationsAssembly(migrationsAssembly);
					});
				})
				.AddAspNetIdentity<ApplicationUser>();

			if (HostingEnvironment.IsProduction())
			{
				// create the signing cred
				identityConfig.AddTemporarySigningCredential();
			}
			else
			{
				identityConfig.AddTemporarySigningCredential();
			}
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			// rewrite http to https
			app.UseRewriter(new RewriteOptions().AddRedirectToHttps());

			if (env.IsDevelopment())
			{
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

			app.UseStaticFiles(new StaticFileOptions()
			{
				ServeUnknownFileTypes = true,
			});

			app.UseIdentity();

			// Adds IdentityServer
			app.UseIdentityServer();

			// Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}

		private void InitializeDatabase(IApplicationBuilder app)
		{
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
}
