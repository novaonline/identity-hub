using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using IdentityServer.Services;
using Microsoft.Extensions.Logging;
using IdentityServer4.Services;
using IdentityServ.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;

namespace IdentityServer.Controllers
{
	[Authorize]
	public class BaseAccountController : Controller
	{
		protected readonly UserManager<ApplicationUser> _userManager;
		protected readonly SignInManager<ApplicationUser> _signInManager;
		protected static readonly string _alertMessageKey = "promptClose";

		public BaseAccountController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		public static string PROMPT_CLOSE_KEY => _alertMessageKey;

		protected void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		protected IActionResult RedirectToLocal(string returnUrl, bool promptClose = true)
		{
			// TODO: Create a unit test for this
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			else
			{
				RouteValueDictionary routeValues = new RouteValueDictionary();
				if (promptClose) routeValues.Add(PROMPT_CLOSE_KEY, true.ToString().ToLower());
				return RedirectToAction(nameof(HomeController.Index), "Home", routeValues);
			}
		}
	}
}