﻿using IdentityServ.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer.Tests
{
    public class FakeSignInManager : SignInManager<ApplicationUser>
    {
		public FakeSignInManager() : base(new Mock<FakeUserManager>().Object,
			new HttpContextAccessor(),
			new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
			new Mock<IOptions<IdentityOptions>>().Object,
			new Mock<ILogger<SignInManager<ApplicationUser>>>().Object)
		{

		}

	}
}
