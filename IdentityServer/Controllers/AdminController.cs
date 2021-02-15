using IdentityServ.Models;
using IdentityServer.Models.IdentityConfigurationViewModels;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static IdentityModel.OidcConstants;

namespace IdentityServer.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly ILogger<AdminController> logger;
		private readonly ConfigurationDbContext identityConfigurationContext;

		public AdminController(
			RoleManager<IdentityRole> roleManager,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			ILogger<AdminController> logger,
			ConfigurationDbContext identityConfigurationContext
			)
		{
			this.roleManager = roleManager;
			this.userManager = userManager;
			this.signInManager = signInManager;
			this.logger = logger;
			this.identityConfigurationContext = identityConfigurationContext;
		}

		[HttpGet]
		public async Task<IActionResult> Index(string searchTerm = null)
		{
			var clients = await identityConfigurationContext.Clients
							.Include(x => x.AllowedScopes)
							.Include(x => x.RedirectUris)
							.Include(x => x.PostLogoutRedirectUris)
							.Include(x => x.AllowedCorsOrigins).ToListAsync();
			var vm = new IdentityConfigurationViewModel()
			{
				Clients = clients
			};
			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> AddClient(IdentityConfigurationViewModel rootModel)
		{
			var model = rootModel.AddClientViewModel;
			if (ModelState.IsValid)
			{
				// First add the new allowed scopes

				var scopedItems = new List<string> {
					IdentityServerConstants.StandardScopes.OpenId,
					IdentityServerConstants.StandardScopes.Email,
				};
				if (model.IncludeProfile)
				{
					scopedItems.Add(IdentityServerConstants.StandardScopes.Profile);
				}
				var apiResourcesSplit = model.AllowedApiResourcesCommaSeperated.Split(",");
				var responsesAfterCreate = new List<ResponseAfterCreate>();
				foreach (var apiResource in apiResourcesSplit)
				{
					var name = apiResource.Trim().Split(" ").First();
					var description = name;
					if (!(await identityConfigurationContext.ApiResources.AnyAsync(x => x.Name == name)))
					{
						var secretPlainText = Guid.NewGuid().ToString();
						var secret = secretPlainText.Sha256();
						var scopes = new List<ApiScope>
							{
								new ApiScope(name:$"{name}.read", displayName: $"{name} read access"),
								new ApiScope(name:$"{name}.write", displayName: $"{name} write access"),
								new ApiScope(name:$"{name}.delete", displayName: $"{name} delete access"),
							};
						if(model.AllowAdminScope)
						{
							var adminScope = new ApiScope(name: $"{name}.manage", displayName: $"Admin Access to {name}");
							scopes.Add(adminScope);
						}
						identityConfigurationContext.ApiScopes.AddRange(scopes.Select(s => s.ToEntity()));
						scopedItems.AddRange(scopes.Select(x => x.Name));
						var apiResourceModel = new ApiResource(name, description)
						{
							Scopes = scopes.Select(x => x.Name).ToList()
						};
						var responseAfterCreateModel = new ResponseAfterCreate() { ApiScopeName = name };
						if (model.RequireSecret)
						{
							responseAfterCreateModel.Secret = secret;
							responseAfterCreateModel.SecretPlainText = secretPlainText;
						}
						identityConfigurationContext.ApiResources.Add(apiResourceModel.ToEntity());
						responsesAfterCreate.Add(responseAfterCreateModel);
					}
				}
				await identityConfigurationContext.SaveChangesAsync();


				var clientModel = new Client()
				{
					ClientId = Regex.Replace(model.ClientName.ToLower(), @"[\W\-]+", "_"),
					ClientName = model.ClientName,
					AllowedGrantTypes = model.GrantType == Models.IdentityConfigurationViewModels.GrantType.Code
					? IdentityServer4.Models.GrantTypes.Code
					: IdentityServer4.Models.GrantTypes.ClientCredentials,
					RequireClientSecret = model.RequireSecret,
					AllowAccessTokensViaBrowser = model.AllowAccessTokenViaBrowser,
					RequireConsent = model.RequireConsent,
					RedirectUris = new List<string> { new UriBuilder(model.BaseUrl) { Path = model.RedirectPath }.ToString() },
					PostLogoutRedirectUris = new List<string> { new UriBuilder(model.BaseUrl) { Path = model.LogoutPath }.ToString() },
					AllowedCorsOrigins = new List<string> { model.BaseUrl.GetLeftPart(UriPartial.Authority) },
					AllowedScopes = scopedItems,
				};
				if(clientModel.RequireClientSecret)
				{
					clientModel.ClientSecrets = responsesAfterCreate.Select(x => new Secret(x.Secret)).ToArray();
				}
				identityConfigurationContext.Clients.Add(clientModel.ToEntity());
				await identityConfigurationContext.SaveChangesAsync();
				var vm = new IdentityConfigurationViewModel
				{
					ResponsesAfterCreate = responsesAfterCreate
				};
				return View(vm);
			}
			return View(new IdentityConfigurationViewModel());
		}

		[HttpPost]
		public IActionResult RemoveClient()
		{
			return View();
		}
	}
}
