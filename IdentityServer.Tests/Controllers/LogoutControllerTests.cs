using IdentityServ.Models;
using IdentityServer.Controllers;
using IdentityServer.Tests.Helpers;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.Tests.Controllers
{
	/// <summary>
	/// Might benefit to register this class into DI so that it is easily testable
	/// 
	/// </summary>
    public class LogoutControllerTests
    {
		//[Fact]
		//public async Task Should_ReturnAView_ShouldShowPromptIsTrue()
		//{
		//	// arrange 
		//	var users = new List<ApplicationUser>()
		//	{
		//		new ApplicationUser()
		//		{

		//		}
		//	};
		//	var controller = new LogoutController(
		//		userManager: AccountMocking.MockFakeUserManager(users).Object,
		//		signInManager: AccountMocking.MockFakeSigninManager(users).Object,
		//		httpContextAccessor: new Mock<IHttpContextAccessor>().Object,
		//		interaction: new Mock<IIdentityServerInteractionService>().Object,
		//		clientStore: new Mock<IClientStore>().Object,
		//		logger: AccountMocking.MockFakeLogger<LogoutController>()
		//		)
		//	{
		//		Url = new Mock<IUrlHelper>().Object
		//	};

		//	// act 
		//	var result = await controller.Index("123");
		//	var viewResult = Assert.IsType<ViewResult>(result);
		//	Assert.True(ImpliedRules.ViewIsActionName(viewResult.ViewName));
		//	Assert.NotNull(viewResult.Model);

		//}
	}
}
