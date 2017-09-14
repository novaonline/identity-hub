using IdentityServ.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer.Tests
{
	/// <summary>
	/// https://samueleresca.net/2017/03/unit-testing-asp-net-core-identity/
	/// </summary>
	public class FakeUserManager : UserManager<ApplicationUser>
    {
		public FakeUserManager() : base(new Mock<IUserStore<ApplicationUser>>().Object,
			new Mock<IOptions<IdentityOptions>>().Object,
			new Mock<IPasswordHasher<ApplicationUser>>().Object,
			new IUserValidator<ApplicationUser>[0],
			new IPasswordValidator<ApplicationUser>[0],
			new Mock<ILookupNormalizer>().Object,
			new Mock<IdentityErrorDescriber>().Object,
			new Mock<IServiceProvider>().Object,
			new Mock<ILogger<UserManager<ApplicationUser>>>().Object
			)
		{

		}
    }
}
