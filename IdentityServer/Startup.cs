﻿using Microsoft.AspNetCore.Builder;
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
using Microsoft.Extensions.Hosting;
using IdentityServer.Helpers.Migration;
using IdentityServer.Helpers.Filters;

namespace IdentityServer
{
	public class Startup
	{
		public IConfiguration Configuration { get; }
		public ILogger Logger { get; }
		public IWebHostEnvironment Env { get;  }

		public Startup(IConfiguration configuration, ILoggerFactory logger, IWebHostEnvironment env)
		{
			Configuration = configuration;
			Logger = logger.CreateLogger("start up");
			Env = env;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllersWithViews();
			services.AddRazorPages().AddRazorRuntimeCompilation();

			services.AddDbContext<ApplicationDbContext>(builder => builder.UseDefault(Configuration));

			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();
			if (Env.IsDevelopment())
			{
				services.Configure<IdentityOptions>(options =>
				{
					options.Password.RequireDigit = true;
					options.Password.RequireLowercase = true;
					options.Password.RequireUppercase = true;
					options.Password.RequiredUniqueChars = 0;
				});
			}

			services.ConfigureApplicationCookie(options =>
			{
				options.ExpireTimeSpan = TimeSpan.FromDays(7);
				options.LoginPath = "/login";
				options.LogoutPath = "/logout";
				options.SlidingExpiration = true;
			});

			// Add application services.
			services.AddTransient<IEmailSender, AuthMessageSender>();
			services.AddTransient<ISmsSender, AuthMessageSender>();


			var mvcBuilder = services.AddMvc(config =>
			{
				// globally authorize each controller just in case an authorize decor was missed
				var policy = new AuthorizationPolicyBuilder()
				 .RequireAuthenticatedUser()
				 .Build();
				config.Filters.Add(new AuthorizeFilter(policy));
			});

			mvcBuilder.AddMvcOptions(o => { o.Filters.Add(new GlobalExceptionFilter(Logger)); });

			// Adds IdentityServer
			var identityBuilder = services.AddIdentityServer(o =>
				{
					o.Events.RaiseErrorEvents = true;
					o.Events.RaiseInformationEvents = true;
					o.Events.RaiseFailureEvents = true;
					o.Events.RaiseSuccessEvents = true;
					o.UserInteraction.LoginUrl = "/login";
					o.UserInteraction.LogoutUrl = "/logout";
				})
				.AddConfigurationStore(options =>
				{
					options.ConfigureDbContext = buider => buider.UseDefault(Configuration);
					options.DefaultSchema = "IdentityConfigure";
				})
				.AddOperationalStore(options =>
				{
					options.ConfigureDbContext = builder => builder.UseDefault(Configuration);
					options.DefaultSchema = "IdentityOperation";
					// this enables automatic token cleanup. this is optional.
					options.EnableTokenCleanup = true;
					options.TokenCleanupInterval = 30;
				}).AddAspNetIdentity<ApplicationUser>();

			if (Env.IsDevelopment())
			{
				identityBuilder.AddDeveloperSigningCredential();
			} else
			{
				// TODO
			}
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app)
		{
			app.UseStaticFiles();

			app.InitializeDatabase(Env);

			if (Env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
			}
			else
			{
				app.UseDeveloperExceptionPage();
				//app.UseExceptionHandler("/Home/Error");
			}

			app.UseRouting();
			app.UseHttpsRedirection();
			app.UseIdentityServer();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapDefaultControllerRoute();
			});
			app.UseHttpsRedirection();
		}
	}
}
