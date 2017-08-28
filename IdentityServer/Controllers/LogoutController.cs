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
			ILoggerFactory loggerFactory) : base(userManager, signInManager)
		{
			_account = new AccountService(interaction, httpContextAccessor, clientStore);
			_logger = loggerFactory.CreateLogger<LogoutController>();

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
			LoggedOutViewModel vm = new LoggedOutViewModel();
			if (model != null)
			{
				vm = await _account.BuildLoggedOutViewModelAsync(model.LogoutId);
			}
			// social login part

			await _signInManager.SignOutAsync();
			_logger.LogInformation(4, "User logged out.");
			return View("LoggedOut", vm);
		}

	}
}