using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityServer.Models.AccountViewModels;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using IdentityServ.Models;
using Microsoft.Extensions.Logging;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IdentityServer.Controllers
{
	public class LogoutController : BaseAccountController
	{
		private readonly AccountService _account;
		private readonly ILogger _logger;

		public LogoutController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			IHttpContextAccessor httpContextAccessor,
			IIdentityServerInteractionService interaction,
			IClientStore clientStore,
			ILogger<LogoutController> logger) : base(userManager, signInManager)
		{
			_account = new AccountService(interaction, httpContextAccessor, clientStore);
			_logger = logger;

		}

		// GET: /Logout
		[HttpGet]
		public async Task<IActionResult> Index(string logoutId)
		{
			var vm = await _account.BuildLogoutViewModelAsync(logoutId);
			if (vm.ShowLogoutPrompt == false)
			{
				// no need to show prompt
				return await Index(vm);
			}

			return View(vm);
		}

		// POST: /Logout
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(LogoutInputModel model = null)
		{
			var prop = new AuthenticationProperties()
			{
				RedirectUri = "/"
			};
			await _signInManager.SignOutAsync();
			_logger.LogInformation("User logged out.");

			// social login part
			LoggedOutViewModel vm = new LoggedOutViewModel()
			{
				AutomaticRedirectAfterSignOut = true,
				PostLogoutRedirectUri = "/",
				ClientName = "Identity Server"
			};
			if (model != null && model.LogoutId != null)
			{
				vm = await _account.BuildLoggedOutViewModelAsync(model.LogoutId);
			}
			
			return View("LoggedOut", vm);
		}

	}
}