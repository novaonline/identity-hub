using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServ.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IdentityServer.Models.AccountViewModels;

namespace IdentityServer.Controllers
{
    public class ResetPasswordController : BaseAccountController
	{
		public ResetPasswordController(
			UserManager<ApplicationUser> userManager, 
			SignInManager<ApplicationUser> signInManager) : base(userManager, signInManager)
		{
		}

		// GET: /ResetPassword
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Index(string code = null)
		{
			return code == null ? View("Error") : View();
		}

		// POST: /ResetPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				// Don't reveal that the user does not exist
				return RedirectToAction(nameof(ResetPasswordController.Confirmation));
			}
			var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
			if (result.Succeeded)
			{
				return RedirectToAction(nameof(ResetPasswordController.Confirmation));
			}
			AddErrors(result);
			return View();
		}

		// GET: /ResetPassword/Confirmation
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Confirmation()
		{
			return View();
		}
	}
}