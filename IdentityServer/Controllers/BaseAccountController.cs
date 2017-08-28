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

namespace IdentityServer.Controllers
{
	[Authorize]
	public class BaseAccountController : Controller
	{
		protected readonly UserManager<ApplicationUser> _userManager;
		protected readonly SignInManager<ApplicationUser> _signInManager;

		public BaseAccountController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		protected void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		protected IActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			else
			{
				return RedirectToAction(nameof(HomeController.Index), "Home");
			}
		}
	}
}