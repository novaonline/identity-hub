using Microsoft.Extensions.Logging;
using IdentityServ.Models;
using IdentityServer.Controllers;
using IdentityServer.Services;
using IdentityServer.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using IdentityServer.Models.AccountViewModels;

namespace IdentityServer.Tests.Controllers
{
	public class RegisterControllerTests
	{
		[Fact]
		public void Should_ReturnAViewResultAndViewData_When_GETRequest()
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

			var controller = new RegisterController(
				userManager: AccountMocking.MockFakeUserManager(users).Object,
				signInManager: AccountMocking.MockFakeSigninManager(users).Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<RegisterController>()
			);
			var returnUrl = "http://www.google.com";

			// act
			var result = controller.Index(returnUrl);

			// assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.Equal(returnUrl, viewResult.ViewData[RegisterController.RETURN_URL_KEY]);
		}

		[Fact]
		public async Task Should_ReturnARedirect_When_RegistrationSucceeds()
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

			var mockUserManager = AccountMocking.MockFakeUserManager(users);
			mockUserManager.Setup(f => f.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);


			var controller = new RegisterController(
				userManager: mockUserManager.Object,
				signInManager: AccountMocking.MockFakeSigninManager(users).Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<RegisterController>()
			)
			{
				Url = new Mock<IUrlHelper>().Object
			};
			var returnUrl = "http://www.google.com";
			RegisterViewModel registerViewModel = new RegisterViewModel();

			// act -- assume ViewModel information is valid 
			var result = await controller.Index(registerViewModel, returnUrl);

			// assert
			var redirectResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal("Home", redirectResult.ControllerName);
			Assert.Equal("true", redirectResult.RouteValues[BaseAccountController.PROMPT_CLOSE_KEY]);
		}

		[Fact]
		public async Task Should_ReturnAView_When_InvalidModelState()
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

			var controller = new RegisterController(
				userManager: AccountMocking.MockFakeUserManager(users).Object,
				signInManager: AccountMocking.MockFakeSigninManager(users).Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<RegisterController>()
			)
			{
				Url = new Mock<IUrlHelper>().Object
			};
			var returnUrl = "http://www.google.com";
			RegisterViewModel registerViewModel = new RegisterViewModel();
			controller.ViewData.ModelState.AddModelError("test", "");

			// act -- assume ViewModel information is valid 
			var result = await controller.Index(registerViewModel, returnUrl);

			// assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.True(!viewResult.ViewData.ModelState.IsValid); // has errors
			Assert.Equal(viewResult.Model, registerViewModel);
			Assert.True(String.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName.Equals("Index"));
		}

		[Fact]
		public async Task Should_ReturnAView_When_ValidModelStateButRegisterFails()
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

			var mockUserManager = AccountMocking.MockFakeUserManager(users);
			mockUserManager.Setup(f => f.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Failed(new IdentityError() { Code = "test", Description = "Test Error" }));

			var controller = new RegisterController(
				userManager: mockUserManager.Object,
				signInManager: AccountMocking.MockFakeSigninManager(users).Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<RegisterController>()
			)
			{
				Url = new Mock<IUrlHelper>().Object
			};
			var returnUrl = "http://www.google.com";
			RegisterViewModel registerViewModel = new RegisterViewModel();

			// act -- assume ViewModel information is valid 
			var result = await controller.Index(registerViewModel, returnUrl);

			// assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.True(!viewResult.ViewData.ModelState.IsValid); // has errors
			Assert.Equal(viewResult.Model, registerViewModel);
			Assert.True(String.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName.Equals("Index"));
		}
	}
}
