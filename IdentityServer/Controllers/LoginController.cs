using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IdentityServer.Models.AccountViewModels;
using IdentityServ.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using IdentityServer.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace IdentityServer.Controllers
{
	public class LoginController : BaseAccountController
	{
		private static readonly string _returnUrlKey = "ReturnUrl";
		private static readonly string _modelStateCredentialsKey = "credentials";
		private readonly string _externalCookieScheme;
		private readonly ILogger _logger;
		private readonly IEmailSender _emailSender;
		private readonly ISmsSender _smsSender;

		public LoginController(
			IOptions<IdentityCookieOptions> identityCookieOptions,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			ILogger<LoginController> logger,
			IEmailSender emailSender,
			ISmsSender smsSender
			) : base(userManager, signInManager)
		{
			_externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
			_emailSender = emailSender;
			_smsSender = smsSender;
			_logger = logger;
		}

		// never seen this before...
		public static string RETURN_URL_KEY => _returnUrlKey;
		public static string MODELSTATE_CREDENTIALS_KEY => _modelStateCredentialsKey;

		// GET: /login
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Index(string returnUrl = null)
		{
			// Clear the existing external cookie to ensure a clean login process
			await _signInManager.SignOutAsync();

			ViewData[RETURN_URL_KEY] = returnUrl;
			return View();
		}

		// POST: /login
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(LoginViewModel model, string returnUrl = null)
		{
			ViewData[RETURN_URL_KEY] = returnUrl;
			if (ModelState.IsValid)
			{
				// This doesn't count login failures towards account lockout
				// To enable password failures to trigger account lockout, set lockoutOnFailure: true
				// we can use email or username because a username cannot have an @ symbol
				ApplicationUser requestedUser = null;
				if (model.Identity.Contains('@'))
				{
					requestedUser = await _userManager.FindByEmailAsync(model.Identity);
				}
				else
				{
					requestedUser = await _userManager.FindByNameAsync(model.Identity);
				}
				var result = await _signInManager.PasswordSignInAsync(requestedUser, model.Password, model.RememberMe, lockoutOnFailure: false);
				if (result.Succeeded)
				{
					_logger.LogInformation(1, "User logged in.");
					return RedirectToLocal(returnUrl);
				}
				if (result.RequiresTwoFactor)
				{
					return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
				}
				if (result.IsLockedOut)
				{
					_logger.LogWarning(2, "User account locked out.");
					return View("Lockout");
				}
				else
				{
					ModelState.AddModelError(MODELSTATE_CREDENTIALS_KEY, "Invalid login attempt.");
					return View(model);
				}
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		// GET: /Login/SendCode
		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
		{
			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user == null)
			{
				return View("Error");
			}
			var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
			var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
			return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
		}

		// POST: /login/SendCode
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SendCode(SendCodeViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}

			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user == null)
			{
				return View("Error");
			}

			// Generate the token and send it
			var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
			if (string.IsNullOrWhiteSpace(code))
			{
				return View("Error");
			}

			var message = "Your security code is: " + code;
			if (model.SelectedProvider == "Email")
			{
				await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
			}
			else if (model.SelectedProvider == "Phone")
			{
				await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
			}

			return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
		}

		// GET: /login/VerifyCode
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
		{
			// Require that the user has already logged in via username/password or external login
			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user == null)
			{
				return View("Error");
			}
			return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
		}

		// POST: /login/VerifyCode
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// The following code protects for brute force attacks against the two factor codes.
			// If a user enters incorrect codes for a specified amount of time then the user account
			// will be locked out for a specified amount of time.
			var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
			if (result.Succeeded)
			{
				return RedirectToLocal(model.ReturnUrl);
			}
			if (result.IsLockedOut)
			{
				_logger.LogWarning(7, "User account locked out.");
				return View("Lockout");
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Invalid code.");
				return View(model);
			}
		}
	}
}