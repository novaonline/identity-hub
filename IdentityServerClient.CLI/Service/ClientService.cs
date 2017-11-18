using IdentityServ.Rules;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static IdentityModel.OidcConstants;

namespace IdentityServerClient.CLI.Service
{
	public class ClientService
	{
		public static int Add(Options.AddOptions options)
		{
			var storeOptions = new IdentityServer4.EntityFramework.Options.ConfigurationStoreOptions();
			var contextOptions = new Microsoft.EntityFrameworkCore.DbContextOptions<ConfigurationDbContext>();
			using (var db = new IdentityConfigurationContext(contextOptions, storeOptions, options))
			{
				Client c = new Client()
				{
					ClientId = options.Name.Replace(" ", "_"),
					ClientName = options.Name,
					AllowedGrantTypes = IdentityServer4.Models.GrantTypes.Implicit,
					AllowAccessTokensViaBrowser = true,
					RequireClientSecret = false,
					AccessTokenType = AccessTokenType.Jwt,
					RedirectUris =
					{
						$"{options.BaseUrl}",
						$"{options.BaseUrl}/index.html",
						$"{options.BaseUrl}/signin-oidc",
						$"{options.BaseUrl}/signin-oidc.html",
						$"{options.BaseUrl}/silent",
						$"{options.BaseUrl}/silent.html",
						$"{options.BaseUrl}/popup",
						$"{options.BaseUrl}/popup.html",
						$"{options.BaseUrl}/callback",
						$"{options.BaseUrl}/callback.html",

					},
					FrontChannelLogoutUri = $"{options.BaseUrl}/signout-oidc",
					PostLogoutRedirectUris =
					{
						$"{options.BaseUrl}/signout-callback-oidc",
						$"{options.BaseUrl}/index.html"
					},
					AllowedCorsOrigins = { $"{options.BaseUrl}" },
					AllowedScopes =
					{
						IdentityServer4.IdentityServerConstants.StandardScopes.OpenId,
						IdentityServer4.IdentityServerConstants.StandardScopes.Profile,
					},
					RequireConsent = false,
				};
				foreach (var scope in options.Scopes)
				{
					c.AllowedScopes.Add(scope);
				}
				db.Clients.Add(c.ToEntity());
				db.SaveChanges();
			}
			return 0;
		}

		public static int Remove(Options.RemoveOptions options)
		{
			throw new NotImplementedException();
		}

		public static int Update(Options.UpdateOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
