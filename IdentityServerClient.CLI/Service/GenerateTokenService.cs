using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServerClient.CLI.Service
{
	public class GenerateTokenService
	{
		public static async Task<int> GenerateAsync(Options.GenerateTokenOptions generateTokenOptions)
		{
			ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
			{
				builder.AddConsole();
			});
			ILogger logger = loggerFactory.CreateLogger<GenerateTokenService>();
			logger.LogInformation("Started Application");
			Thread.Sleep(1000);
			var baseUrl = generateTokenOptions.BaseUrl;
			if(string.IsNullOrEmpty(baseUrl))
			{
				Console.Write("BaseUrl: ");
				baseUrl = Console.ReadLine();
			}
			var clientSecret = generateTokenOptions.ClientSecret;
			if (string.IsNullOrEmpty(clientSecret))
			{
				Console.Write("ClientSecret: ");
				clientSecret = Console.ReadLine();
			}
			var scope = generateTokenOptions.Scope;
			if(string.IsNullOrEmpty(scope))
			{
				Console.Write("Scope: ");
				scope = Console.ReadLine();
			}
			var client = new HttpClient();
			var discovery = await client.GetDiscoveryDocumentAsync(baseUrl);
			if(discovery.IsError)
			{
				logger.LogError(discovery.Error);
				return -1;
			}
			var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
			{
				Address = discovery.TokenEndpoint,
				ClientId = generateTokenOptions.ClientId,
				ClientSecret = clientSecret,
				Scope = scope
			});
			if(tokenResponse.IsError)
			{
				logger.LogError(tokenResponse.Error);
				return -1;
			}
			Console.WriteLine(tokenResponse.Json);
			return 0;
		}
	}
}
