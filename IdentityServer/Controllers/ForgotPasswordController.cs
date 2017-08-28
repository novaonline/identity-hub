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
    public class ForgotPasswordController : BaseAccountController
	{
		public ForgotPasswordController(
			UserManager<ApplicationUser> userManager, 
			SignInManager<ApplicationUser> signInManager) : base(userManager, signInManager)
		{
		}

		// GET: /ForgotPassword
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Index()
		{
			return View();
		}

		//
		// POST: /ForgotPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
				{
					// Don't reveal that the user does not exist or is not confirmed
					return View("Confirmation");
				}

				// For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
				// Send an email with this link
				//var code = await _userManager.GeneratePasswordResetTokenAsync(user);
				//var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
				//await _emailSender.SendEmailAsync(model.Email, "Reset Password",
				//   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
				//return View("ForgotPasswordConfirmation");
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		//
		// GET: /ForgotPassword/Confirmation
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Confirmation()
		{
			return View();
		}
	}
}