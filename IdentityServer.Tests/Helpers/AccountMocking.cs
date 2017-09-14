﻿using IdentityServ.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IdentityServer.Tests.Helpers
{
    public class AccountMocking
    {
		public static Mock<FakeUserManager> MockFakeUserManager(IEnumerable<ApplicationUser> users)
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

		public static Mock<FakeSignInManager> MockFakeSigninManager(
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

		public static Mock<IOptions<IdentityCookieOptions>> MockFakeIdentityCookieOptions()
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
	}
}
