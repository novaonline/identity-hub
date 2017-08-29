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
	/// </summary>
	public class Config
	{
		// scopes define the API resources in your system
		public static IEnumerable<ApiResource> GetApiResources()
		{
			return new List<ApiResource>
			{
				// database this
                new ApiResource("RealTimeReality", "Real Time Reality API")
			};
		}

		// client want to access resources (aka scopes)
		public static IEnumerable<Client> GetClients()
		{
			// database these 
			string CLIENT_HOST_IP = "localhost";
			return new List<Client>
						{
							new Client
							{
								ClientId = "client",
								AllowedGrantTypes = GrantTypes.ClientCredentials,

								ClientSecrets =
								{
									new Secret("secret".Sha256())
								},
								AllowedScopes = { "RealTimeReality" }
							},
							new Client
							{
								ClientId = "mvc",
								ClientName = "MVC Client",
								AllowedGrantTypes = GrantTypes.Implicit,

								RequireConsent = false,

								ClientSecrets =
								{
									new Secret("secret".Sha256())
								},

								RedirectUris           = { "http://localhost:5002/signin-oidc" },
								PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

								AllowedScopes =
								{
									IdentityServerConstants.StandardScopes.OpenId,
									IdentityServerConstants.StandardScopes.Profile,
									"RealTimeReality"
								},
								AllowOfflineAccess = true
							},
							// JavaScript Client
							new Client
							{
								ClientId = "js_oidc",
								ClientName = "JavaScript Client",
								AllowedGrantTypes = GrantTypes.Implicit,
								AllowAccessTokensViaBrowser = true,
								RequireConsent = false,


								RedirectUris = { $"http://{CLIENT_HOST_IP}:5005/callback.html" },
								PostLogoutRedirectUris = { $"http://{CLIENT_HOST_IP}:5005/index.html" },
								AllowedCorsOrigins = { $"http://{CLIENT_HOST_IP}:5005" },

								AllowedScopes =
								{
									IdentityServerConstants.StandardScopes.OpenId,
									IdentityServerConstants.StandardScopes.Profile,
									IdentityServerConstants.StandardScopes.Email,
									"RealTimeReality"
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
			};
		}
	}
}
