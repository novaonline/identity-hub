using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace IdentityServer.Tests
{
	public class TestFixture<TStartup> : IDisposable where TStartup : class
	{
		public readonly TestServer Server;
		private readonly HttpClient _client;

		public TestFixture()
		{
			var builder = new WebHostBuilder()
				.UseContentRoot($"..\\..\\..\\..\\IdentityServer\\")
				.UseStartup<TStartup>();

			Server = new TestServer(builder);
			_client = new HttpClient();
		}

		public void Dispose()
		{
			_client.Dispose();
			Server.Dispose();
		}
	}
}
