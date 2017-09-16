using IdentityServ.Models;
using IdentityServer.Controllers;
using IdentityServer.Services;
using IdentityServer.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Controllers
{
	public class ManageControllerTests
	{
		[Fact]
		public async Task Should_ReturnViewWithModel_When_GetRequest()
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
			var currentUser = users[0];
			var fakeUserManager = AccountMocking.MockFakeUserManager(users);
			fakeUserManager.Setup(f => f.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				.ReturnsAsync(currentUser);
			fakeUserManager.Setup(f => f.HasPasswordAsync(It.IsAny<ApplicationUser>()))
				.ReturnsAsync(true);
			fakeUserManager.Setup(f => f.GetPhoneNumberAsync(It.IsAny<ApplicationUser>()))
				.ReturnsAsync("");
			fakeUserManager.Setup(f => f.GetTwoFactorEnabledAsync(It.IsAny<ApplicationUser>()))
				.ReturnsAsync(false);
			fakeUserManager.Setup(f => f.GetLoginsAsync(It.IsAny<ApplicationUser>()))
				.ReturnsAsync(new List<UserLoginInfo>());

			var fakeSigninManager = AccountMocking.MockFakeSigninManager(users);
			fakeSigninManager.Setup(f => f.IsTwoFactorClientRememberedAsync(It.IsAny<ApplicationUser>()))
				.ReturnsAsync(true);

			var controller = new ManageController(
				userManager: fakeUserManager.Object,
				signInManager: fakeSigninManager.Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<ManageController>()
			);

			// act
			var result = await controller.Index();
			var resultWithMessage = await controller.Index(ManageController.ManageMessageId.Error);

			// assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.NotNull(viewResult.Model);
			Assert.True(ImpliedRules.ViewIsActionName(viewResult.ViewName));

			var viewResultWithMessage = Assert.IsType<ViewResult>(resultWithMessage);
			Assert.NotNull(viewResult.Model);
			Assert.True(ImpliedRules.ViewIsActionName(viewResultWithMessage.ViewName));
		}

		[Fact]
		public async Task Should_ReturnErrorView_When_GetRequestWithNoCurrentUser()
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
			var fakeUserManager = AccountMocking.MockFakeUserManager(users);
			fakeUserManager.Setup(f => f.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				.ReturnsAsync(value: null);

			var controller = new ManageController(
				userManager: fakeUserManager.Object,
				signInManager: AccountMocking.MockFakeSigninManager(users).Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<ManageController>()
			);

			// act
			var result = await controller.Index();

			// assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.Null(viewResult.Model);
			Assert.Equal("Error", viewResult.ViewName);
		}

		[Fact]
		public void ChangePassword_Should_ReturnView_When_GetRequest()
		{
			var controller = new ManageController(
				userManager: new Mock<FakeUserManager>().Object,
				signInManager: new Mock<FakeSignInManager>().Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<ManageController>()
			);

			// act
			var result = controller.ChangePassword();
			
			// assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.Null(viewResult.Model);
			Assert.True(ImpliedRules.ViewIsActionName(viewResult.ViewName));
		}

		[Fact]
		public async Task ChangePassword_Should_ReturnCurrentView_When_InvalidPostToChangePassword()
		{
			// arrange
			var controller = new ManageController(
				userManager: new Mock<FakeUserManager>().Object,
				signInManager: new Mock<FakeSignInManager>().Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<ManageController>()
			);
			// fake up the error
			controller.ViewData.ModelState.AddModelError("test", "");

			// act
			var result = await controller.ChangePassword(new Models.ManageViewModels.ChangePasswordViewModel());

			// assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.NotNull(viewResult.Model);
			Assert.True(ImpliedRules.ViewIsActionName(viewResult.ViewName));
		}

		[Fact]
		public async Task ChangePassword_Should_RedirectToIndexWithErrorMessage_When_CurrentUserCannotBeFetched()
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
			var fakeUserManager = AccountMocking.MockFakeUserManager(users);
			fakeUserManager.Setup(f => f.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				.ReturnsAsync(value: null);

			var controller = new ManageController(
				userManager: fakeUserManager.Object,
				signInManager: AccountMocking.MockFakeSigninManager(users).Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<ManageController>()
			);

			// act
			var result = await controller.ChangePassword(new Models.ManageViewModels.ChangePasswordViewModel());

			// assert
			var redirectResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(controller.Index), redirectResult.ActionName);
			Assert.Equal(ManageController.ManageMessageId.Error, redirectResult.RouteValues["Message"]);
		}

		[Fact]
		public async Task ChangePassword_Should_RedirectToIndexWithErrorMessage_When_CurrentUserCanBeFetchedAndSuccess()
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
			var currentUser = users[0];
			var fakeUserManager = AccountMocking.MockFakeUserManager(users);
			fakeUserManager.Setup(f => f.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				.ReturnsAsync(value: currentUser);
			fakeUserManager.Setup(f => f.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Success);

			var fakeSigninManager = AccountMocking.MockFakeSigninManager(users);
			
			var controller = new ManageController(
				userManager: fakeUserManager.Object,
				signInManager: fakeSigninManager.Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<ManageController>()
			);

			// act
			var result = await controller.ChangePassword(new Models.ManageViewModels.ChangePasswordViewModel());

			// assert
			var redirectResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(controller.Index), redirectResult.ActionName);
			Assert.Equal(ManageController.ManageMessageId.ChangePasswordSuccess, redirectResult.RouteValues["Message"]);
		}

		[Fact]
		public async Task ChangePassword_Should_ReturnViewWithModelAndErrors_When_UpdateFailes()
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
			var currentUser = users[0];
			var fakeUserManager = AccountMocking.MockFakeUserManager(users);
			fakeUserManager.Setup(f => f.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				.ReturnsAsync(value: currentUser);
			fakeUserManager.Setup(f => f.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Failed(new IdentityError() { Code="test", Description="Test Error" }));

			var fakeSigninManager = AccountMocking.MockFakeSigninManager(users);

			var controller = new ManageController(
				userManager: fakeUserManager.Object,
				signInManager: fakeSigninManager.Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<ManageController>()
			);

			// act
			var result = await controller.ChangePassword(new Models.ManageViewModels.ChangePasswordViewModel());

			// assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.True(ImpliedRules.ViewIsActionName(viewResult.ViewName));
			Assert.NotNull(viewResult.Model);
			Assert.Equal("Test Error", viewResult.ViewData.ModelState[String.Empty].Errors[0].ErrorMessage);
		}

		[Fact]
		public void ChangeProfile_Should_ReturnView_When_GetRequested()
		{
			var controller = new ManageController(
				userManager: new Mock<FakeUserManager>().Object,
				signInManager: new Mock<FakeSignInManager>().Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<ManageController>()
			);

			// act
			var result = controller.ChangeProfile();

			// assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.Null(viewResult.Model);
			Assert.True(ImpliedRules.ViewIsActionName(viewResult.ViewName));
		}

		[Fact]
		public async Task ChangeProfile_Should_ReturnCurrentView_When_InvalidPost()
		{
			// arrange
			var controller = new ManageController(
				userManager: new Mock<FakeUserManager>().Object,
				signInManager: new Mock<FakeSignInManager>().Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<ManageController>()
			);
			// fake up the error
			controller.ViewData.ModelState.AddModelError("test", "");

			// act
			var result = await controller.ChangeProfile(new Models.AccountViewModels.ChangeProfileViewModel());

			// assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.NotNull(viewResult.Model);
			Assert.True(ImpliedRules.ViewIsActionName(viewResult.ViewName));
		}

		[Fact]
		public async Task ChangeProfile_Should_RedirectToIndexWithErrorMessage_When_CurrentUserCannotBeFetched()
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
			var fakeUserManager = AccountMocking.MockFakeUserManager(users);
			fakeUserManager.Setup(f => f.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				.ReturnsAsync(value: null);

			var controller = new ManageController(
				userManager: fakeUserManager.Object,
				signInManager: AccountMocking.MockFakeSigninManager(users).Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<ManageController>()
			);

			// act
			var result = await controller.ChangeProfile(new Models.AccountViewModels.ChangeProfileViewModel());

			// assert
			var redirectResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(controller.Index), redirectResult.ActionName);
			Assert.Equal(ManageController.ManageMessageId.Error, redirectResult.RouteValues["Message"]);
		}

		[Fact]
		public async Task ChangeProfile_Should_RedirectToIndexWithErrorMessage_When_CurrentUserCanBeFetchedAndSuccess()
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
			var currentUser = users[0];
			var fakeUserManager = AccountMocking.MockFakeUserManager(users);
			fakeUserManager.Setup(f => f.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				.ReturnsAsync(value: currentUser);
			fakeUserManager.Setup(f => f.UpdateAsync(It.IsAny<ApplicationUser>()))
				.ReturnsAsync(IdentityResult.Success);

			var fakeSigninManager = AccountMocking.MockFakeSigninManager(users);

			var controller = new ManageController(
				userManager: fakeUserManager.Object,
				signInManager: fakeSigninManager.Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<ManageController>()
			);

			// act
			var result = await controller.ChangeProfile(new Models.AccountViewModels.ChangeProfileViewModel());

			// assert
			var redirectResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(controller.Index), redirectResult.ActionName);
			Assert.Equal(ManageController.ManageMessageId.ProfileUpdateSuccess, redirectResult.RouteValues["Message"]);
		}

		[Fact]
		public async Task ChangeProfile_Should_ReturnViewWithModelAndErrors_When_UpdateFailes()
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
			var currentUser = users[0];
			var fakeUserManager = AccountMocking.MockFakeUserManager(users);
			fakeUserManager.Setup(f => f.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				.ReturnsAsync(value: currentUser);
			fakeUserManager.Setup(f => f.UpdateAsync(It.IsAny<ApplicationUser>()))
				.ReturnsAsync(IdentityResult.Failed(new IdentityError() { Code = "test", Description = "Test Error" }));

			var fakeSigninManager = AccountMocking.MockFakeSigninManager(users);

			var controller = new ManageController(
				userManager: fakeUserManager.Object,
				signInManager: fakeSigninManager.Object,
				emailSender: new Mock<IEmailSender>().Object,
				smsSender: new Mock<ISmsSender>().Object,
				logger: AccountMocking.MockFakeLogger<ManageController>()
			);

			// act
			var result = await controller.ChangeProfile(new Models.AccountViewModels.ChangeProfileViewModel());

			// assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.True(ImpliedRules.ViewIsActionName(viewResult.ViewName));
			Assert.NotNull(viewResult.Model);
			Assert.Equal("Test Error", viewResult.ViewData.ModelState[String.Empty].Errors[0].ErrorMessage);
		}

	}
}
