using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Configuration
{
	/// <summary>
	/// NOTE: This is strictly development configurations
	/// https://identityserver4.readthedocs.io/en/latest/topics/grant_types.html
	/// https://oauth.net/2/grant-types/
	/// https://identityserver4.readthedocs.io/en/latest/topics/clients.html
	/// https://oauth.tools/
	/// </summary>
	public class Config
	{
		public static IEnumerable<ApiScope> GetApiScopes()
		{
			return new List<ApiScope>
			{
				new ApiScope(name:"portfolio.read", displayName: "Portfolio read access"),
				new ApiScope(name:"portfolio.write", displayName: "Portfolio write access"),
				new ApiScope(name:"portfolio.delete", displayName: "Portfolio delete access"),
				new ApiScope(name:"manage", displayName: "Admin Access")
			};
		}

		// scopes define the API resources in your system
		public static IEnumerable<ApiResource> GetApiResources()
		{
			return new List<ApiResource>
			{
				// database this
                new ApiResource("Portfolio", "The Portfolio API") {
					ApiSecrets =
					{
						new Secret("secret".Sha256())
					},
					Scopes = GetApiScopes().Where(a => a.Name.StartsWith("portfolio")).Select(x => x.Name).ToList()
				}
				// TODO: add realtime reality
			};
		}

		// client want to access resources (aka scopes)
		public static IEnumerable<Client> GetClients()
		{
			// database these 
			string CLIENT_HOST_IP = "localhost";
			return new List<Client>
						{
							// JavaScript Client
							new Client
							{
								ClientId = "js_oidc",
								ClientName = "JavaScript Client",
								AllowedGrantTypes = GrantTypes.ClientCredentials,
								AllowAccessTokensViaBrowser = true,
								RequireConsent = false,
								RedirectUris = new List<string> { $"http://{CLIENT_HOST_IP}:5005/auth-callback" },
								PostLogoutRedirectUris = new List<string> { $"http://{CLIENT_HOST_IP}:5005/" },
								AllowedCorsOrigins = new List<string> { $"http://{CLIENT_HOST_IP}:5005" },
								AllowedScopes = new List<string> {
									IdentityServerConstants.StandardScopes.OpenId,
									IdentityServerConstants.StandardScopes.Profile,
									IdentityServerConstants.StandardScopes.Email,
									"Portfolio"
								}
							}
				};
		}

		// scopes define the resources in your system
		public static IEnumerable<IdentityResource> GetIdentityResources()
		{
			return new List<IdentityResource>
			{
				new IdentityResources.OpenId(),
				new IdentityResources.Profile(),
				new IdentityResources.Email()
			};
		}
	}
}
