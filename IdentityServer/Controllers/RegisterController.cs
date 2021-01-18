using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServ.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IdentityServer.Models.AccountViewModels;
using Microsoft.Extensions.Logging;
using IdentityServer.Services;

namespace IdentityServer.Controllers
{
    public class RegisterController : BaseAccountController
	{
		private readonly ILogger _logger;
		private readonly IEmailSender _emailSender;
		private static readonly string _returnUrlKey = "ReturnUrl";
		private readonly ISmsSender _smsSender;

		public RegisterController(
			UserManager<ApplicationUser> userManager, 
			SignInManager<ApplicationUser> signInManager,
			ILogger<RegisterController> logger,
			IEmailSender emailSender,
			ISmsSender smsSender
			) : base(userManager, signInManager)
		{
			_logger = logger;
			_emailSender = emailSender;
			_smsSender = smsSender;
		}

		public static string RETURN_URL_KEY => _returnUrlKey;

		//
		// GET: /Register
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Index(string returnUrl = null)
		{
			ViewData[RETURN_URL_KEY] = returnUrl;
			return View();
		}

		//
		// POST: /Register
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(RegisterViewModel model, string returnUrl = null)
		{
			ViewData[RETURN_URL_KEY] = returnUrl;
			if (ModelState.IsValid)
			{
				var user = new ApplicationUser
				{
					UserName = model.UserName,
					Email = model.Email,
					BirthDate = model.BirthDate,
					FirstName = model.FirstName,
					LastName = model.LastName
				};
				var result = await _userManager.CreateAsync(user, model.Password);
				if (result.Succeeded)
				{
					// For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
					// Send an email with this link
					//var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
					//var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
					//await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
					//    $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
					await _signInManager.SignInAsync(user, isPersistent: false);
					_logger.LogInformation(3, "User created a new account with password.");
					return RedirectToLocal(returnUrl);
				}
				AddErrors(result);
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		// GET: /Register/ConfirmEmail
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ConfirmEmail(string userId, string code)
		{
			if (userId == null || code == null)
			{
				return View("Error");
			}
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return View("Error");
			}
			var result = await _userManager.ConfirmEmailAsync(user, code);
			return View(result.Succeeded ? "ConfirmEmail" : "Error");
		}

	}
}