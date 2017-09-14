using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using IdentityServ.Models;
using Microsoft.Extensions.Logging;
using IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using IdentityServer.Controllers;
using System.Collections.Generic;
using System.Linq;
using IdentityServer.Models.AccountViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer.Tests.Controller
{
	/// <summary>
	/// https://stackoverflow.com/questions/40284313/how-do-i-create-an-httpcontext-for-my-unit-test
	/// https://dzone.com/articles/7-popular-unit-test-naming
	/// </summary>
	public class LoginControllerTests : IClassFixture<TestFixture<Startup>>
	{
		// arrange
		public LoginControllerTests(TestFixture<Startup> fixture)
		{
			//var users = new List<ApplicationUser>
			//{
			//	new ApplicationUser
			//	{
			//		UserName = "Test",
			//		Id = Guid.NewGuid().ToString(),
			//		Email = "test@test.it"
			//	}
			//}.AsQueryable();

			//var fakeUserManager = new Mock<FakeUserManager>();

			//fakeUserManager.Setup(p => p.Users).Returns(users);

			//fakeUserManager.Setup(f => f.DeleteAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
			//fakeUserManager.Setup(f => f.CreateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
			//fakeUserManager.Setup(f => f.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
			//fakeUserManager.Setup(f => f.FindByEmailAsync(It.Is<string>(s => s == "test@test.it"))).ReturnsAsync(users.First());
			//fakeUserManager.Setup(f => f.FindByEmailAsync(It.Is<string>(s => s != "test@test.it"))).ReturnsAsync(value: null);
			//fakeUserManager.Setup(f => f.FindByNameAsync(It.Is<string>(s => s == "test"))).ReturnsAsync(users.First());
			//fakeUserManager.Setup(f => f.FindByNameAsync(It.Is<string>(s => s != "test"))).ReturnsAsync(value: null);


			//var uservalidator = new Mock<IUserValidator<ApplicationUser>>();
			//uservalidator.Setup(x => x.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>()))
			// .ReturnsAsync(IdentityResult.Success);
			//var passwordvalidator = new Mock<IPasswordValidator<ApplicationUser>>();
			//passwordvalidator.Setup(x => x.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>(), It.IsAny<string>()))
			// .ReturnsAsync(IdentityResult.Success);

			//var fakeSignInManager = new Mock<FakeSignInManager>();

			//fakeSignInManager.Setup(
			//		x => x.PasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
			//	.ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

			//var fakeIdentityCookieOptions = new Mock<IOptions<IdentityCookieOptions>>();
			//var identityCookieOptions = new IdentityCookieOptions
			//{
			//	ExternalCookie = new Microsoft.AspNetCore.Builder.CookieAuthenticationOptions
			//	{
			//		AutomaticChallenge = true,
			//		AutomaticAuthenticate = true,
			//	},
			//	ApplicationCookie = new Microsoft.AspNetCore.Builder.CookieAuthenticationOptions
			//	{
			//		AutomaticChallenge = true,
			//		AutomaticAuthenticate = true,
			//	}
			//};
			//fakeIdentityCookieOptions.Setup(p => p.Value).Returns(identityCookieOptions);


			//Controller = new LoginController(
			//	identityCookieOptions: fakeIdentityCookieOptions.Object,
			//	userManager: fakeUserManager.Object,
			//	signInManager: fakeSignInManager.Object,
			//	emailSender: new Mock<IEmailSender>().Object,
			//	smsSender: new Mock<ISmsSender>().Object,
			//	loggerFactory: new Mock<ILoggerFactory>().Object
			//	)
			//{

			//};
		}


		private Mock<FakeUserManager> MockFakeUserManager(IEnumerable<ApplicationUser> users)
		{
			var emails = users.Select(x => x.Email);
			var usernames = users.Select(x => x.UserName);

			var fakeUserManager = new Mock<FakeUserManager>();
			fakeUserManager.Setup(p => p.Users).Returns(users.AsQueryable());
			fakeUserManager.Setup(f => f.FindByEmailAsync(It.IsIn(emails))).ReturnsAsync(users.First());
			fakeUserManager.Setup(f => f.FindByEmailAsync(It.IsNotIn(emails))).ReturnsAsync(value: null);
			fakeUserManager.Setup(f => f.FindByNameAsync(It.IsIn(usernames))).ReturnsAsync(users.First());
			fakeUserManager.Setup(f => f.FindByNameAsync(It.IsNotIn(usernames))).ReturnsAsync(value: null);
			return fakeUserManager;
		}

		private Mock<FakeSignInManager> MockFakeSigninManager(
			IEnumerable<ApplicationUser> mockUsersWhoGetLoggedIn,
			IEnumerable<ApplicationUser> mockUsersWhoDoNotGetLoggedIn = null,
			IEnumerable<ApplicationUser> mockUsersWhoAreLockedOut = null,
			bool persistent = false)
		{
			// default to empty
			mockUsersWhoDoNotGetLoggedIn = mockUsersWhoDoNotGetLoggedIn ?? new List<ApplicationUser>();
			mockUsersWhoAreLockedOut = mockUsersWhoAreLockedOut ?? new List<ApplicationUser>();

			var fakeSignInManager = new Mock<FakeSignInManager>();

			fakeSignInManager.Setup(
					x => x.PasswordSignInAsync(It.IsIn(mockUsersWhoGetLoggedIn), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
				.ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

			fakeSignInManager.Setup(
				x => x.PasswordSignInAsync(It.IsIn(mockUsersWhoDoNotGetLoggedIn), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
				.ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

			fakeSignInManager.Setup(
				x => x.PasswordSignInAsync(It.IsIn(mockUsersWhoAreLockedOut), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
				.ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

			return fakeSignInManager;
		}

		private Mock<IOptions<IdentityCookieOptions>> MockFakeIdentityCookieOptions()
		{
			var fakeIdentityCookieOptions = new Mock<IOptions<IdentityCookieOptions>>();
			var identityCookieOptions = new IdentityCookieOptions
			{
				ExternalCookie = new Microsoft.AspNetCore.Builder.CookieAuthenticationOptions
				{
					AutomaticChallenge = true,
					AutomaticAuthenticate = true,
				},
				ApplicationCookie = new Microsoft.AspNetCore.Builder.CookieAuthenticationOptions
				{
					AutomaticChallenge = true,
					AutomaticAuthenticate = true,
				}
			};
			fakeIdentityCookieOptions.Setup(p => p.Value).Returns(identityCookieOptions);
			return fakeIdentityCookieOptions;
		}

		private LoginController MockController()
		{
			var users = new List<ApplicationUser>
			{
				new ApplicationUser
				{
					UserName = "test",
					Id = Guid.NewGuid().ToString(),
					Email = "test@test.it"
				}
			};

			var fakeUserManager = MockFakeUserManager(users);

			var fakeSignInManager = MockFakeSigninManager(users);

			var fakeIdentityCookieOptions = MockFakeIdentityCookieOptions();

			return new LoginController(
				identityCookieOptions: fakeIdentityCookieOptions.Object,
				userManager: fakeUserManager.Object,
				signInManager: fakeSignInManager.Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: new Mock<ILogger<LoginController>>().Object
				)
			{

			};
		}

		[Fact]
		public async Task Should_ReturnAViewResultAndViewData_When_GETRequest()
		{
			// arrange
			var users = new List<ApplicationUser>
			{
				new ApplicationUser
				{
					UserName = "test",
					Id = Guid.NewGuid().ToString(),
					Email = "test@test.it"
				}
			};
			var serviceProvider = new ServiceCollection()
				.AddLogging()
				.BuildServiceProvider();

			var factory = serviceProvider.GetService<ILoggerFactory>();

			var logger = factory.CreateLogger<LoginController>();

			var controller = new LoginController(
				identityCookieOptions: MockFakeIdentityCookieOptions().Object,
				userManager: MockFakeUserManager(users).Object,
				signInManager: MockFakeSigninManager(users).Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: logger
			);
			var returnUrl = "http://www.google.com";

			// act
			var result = await controller.Index(returnUrl);

			// assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.Equal(returnUrl, viewResult.ViewData[LoginController.RETURN_URL_KEY]);
		}

		[Fact]
		public async Task Should_ReturnARedirect_When_ExistingCredentialsPassedToPOST()
		{
			// arrange 
			var users = new List<ApplicationUser>
			{
				new ApplicationUser
				{
					UserName = "test",
					Id = Guid.NewGuid().ToString(),
					Email = "test@test.it"
				}
			};
			var serviceProvider = new ServiceCollection()
				.AddLogging()
				.BuildServiceProvider();

			var factory = serviceProvider.GetService<ILoggerFactory>();

			var logger = factory.CreateLogger<LoginController>();

			var controller = new LoginController(
				identityCookieOptions: MockFakeIdentityCookieOptions().Object,
				userManager: MockFakeUserManager(users).Object,
				signInManager: MockFakeSigninManager(users).Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: logger
			)
			{
				Url = new Mock<IUrlHelper>().Object
			};
			var returnUrl = "http://www.google.com";
			LoginViewModel loginViewModel = new LoginViewModel
			{
				Identity = "test@test.it"
			};
			LoginViewModel loginViewModelWithUserName = new LoginViewModel
			{
				Identity = "test",
				Password = "123",
			};

			// act
			var result = await controller.Index(loginViewModel, returnUrl);
			var resultUsingUsername = await controller.Index(loginViewModelWithUserName, returnUrl);

			// assert
			var viewResult = Assert.IsType<RedirectToActionResult>(result); // because return url is external
			var viewResultUsingUsername = Assert.IsType<RedirectToActionResult>(resultUsingUsername);
			Assert.Equal("Home", viewResult.ControllerName);
			Assert.Equal("Home", viewResultUsingUsername.ControllerName);
		}

		[Fact]
		public async Task Should_ReturnView_When_NonExistingOrBadCredentialsPassedToPOST()
		{
			// arrange 
			var emptyUsers = new List<ApplicationUser>();
			var badCredentialsUsers = new List<ApplicationUser>
			{
				new ApplicationUser
				{
					UserName = "test",
					Id = Guid.NewGuid().ToString(),
					Email = "test@test.it"
				}
			};
			var serviceProvider = new ServiceCollection()
				.AddLogging()
				.BuildServiceProvider();

			var factory = serviceProvider.GetService<ILoggerFactory>();

			var logger = factory.CreateLogger<LoginController>();

			var controller = new LoginController(
				identityCookieOptions: MockFakeIdentityCookieOptions().Object,
				userManager: MockFakeUserManager(badCredentialsUsers).Object,
				signInManager: MockFakeSigninManager(emptyUsers, badCredentialsUsers).Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: logger
			)
			{
				Url = new Mock<IUrlHelper>().Object
			};
			var returnUrl = "http://www.google.com";
			LoginViewModel loginViewModel = new LoginViewModel
			{
				Identity = "test@test.it",
				Password = "123"
			};
			LoginViewModel loginViewModelWithUserName = new LoginViewModel
			{
				Identity = "test",
				Password = "123"
			};

			// act
			var result = await controller.Index(loginViewModel, returnUrl);
			var resultUsingUsername = await controller.Index(loginViewModelWithUserName, returnUrl);

			// assert
			var viewResult = Assert.IsType<ViewResult>(result); // because return url is external
			var viewResultUsingUsername = Assert.IsType<ViewResult>(resultUsingUsername);
			Assert.True(viewResult.ViewData.ModelState[LoginController.MODELSTATE_CREDENTIALS_KEY] != null); // has errors
			Assert.True(viewResultUsingUsername.ViewData.ModelState[LoginController.MODELSTATE_CREDENTIALS_KEY] != null); // has errors
		}

		[Fact]
		public async Task Should_IncludeCredentialsModelError_When_InvalidCredentialsPassedToPOST()
		{
			// arrange 
			var emptyUsers = new List<ApplicationUser>();
			var badCredentialsUsers = new List<ApplicationUser>
			{
				new ApplicationUser
				{
					UserName = "test",
					Id = Guid.NewGuid().ToString(),
					Email = "test@test.it"
				}
			};
			var serviceProvider = new ServiceCollection()
				.AddLogging()
				.BuildServiceProvider();

			var factory = serviceProvider.GetService<ILoggerFactory>();

			var logger = factory.CreateLogger<LoginController>();

			var controller = new LoginController(
				identityCookieOptions: MockFakeIdentityCookieOptions().Object,
				userManager: MockFakeUserManager(badCredentialsUsers).Object,
				signInManager: MockFakeSigninManager(emptyUsers, badCredentialsUsers).Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: logger
			)
			{
				Url = new Mock<IUrlHelper>().Object
			};
			var returnUrl = "http://www.google.com";
			// fake up the error
			controller.ViewData.ModelState.AddModelError("test", "");

			// act
			// we assume that the model binder has sucessfully binded the model errors
			var result = await controller.Index(new LoginViewModel(), returnUrl);

			// assert
			var viewResult = Assert.IsType<ViewResult>(result); // because return url is external
			Assert.True(!viewResult.ViewData.ModelState.IsValid); // has errors
			Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName.Equals("Index"));
		}

		[Fact]
		public async Task Should_ReturnLockedOutViewResult_When_IsLockedOut()
		{
			// arrange 
			var emptyUsers = new List<ApplicationUser>();
			var lockedOutUsers = new List<ApplicationUser>
			{
				new ApplicationUser
				{
					UserName = "test",
					Id = Guid.NewGuid().ToString(),
					Email = "test@test.it"
				}
			};
			var serviceProvider = new ServiceCollection()
				.AddLogging()
				.BuildServiceProvider();

			var factory = serviceProvider.GetService<ILoggerFactory>();

			var logger = factory.CreateLogger<LoginController>();

			var controller = new LoginController(
				identityCookieOptions: MockFakeIdentityCookieOptions().Object,
				userManager: MockFakeUserManager(lockedOutUsers).Object,
				signInManager: MockFakeSigninManager(emptyUsers, emptyUsers, lockedOutUsers).Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: logger
			)
			{
				Url = new Mock<IUrlHelper>().Object
			};
			var returnUrl = "http://www.google.com";
			LoginViewModel loginViewModel = new LoginViewModel
			{
				Identity = "test@test.it",
				Password = "123"
			};
			LoginViewModel loginViewModelWithUserName = new LoginViewModel
			{
				Identity = "test",
				Password = "123"
			};

			// act
			var result = await controller.Index(loginViewModel, returnUrl);
			var resultUsingUsername = await controller.Index(loginViewModelWithUserName, returnUrl);

			// assert
			var viewResult = Assert.IsType<ViewResult>(result); // because return url is external
			var viewResultUsingUsername = Assert.IsType<ViewResult>(resultUsingUsername);
			// wish I could figure out more information about the view passed.
			Assert.Equal(viewResult.ViewName, "Lockout");

		}

		[Fact]
		public void Should_ValidateModelState_When_ViewModelPassedIn()
		{
			// https://stackoverflow.com/questions/286124/how-can-i-test-modelstate
		}
	}
}
