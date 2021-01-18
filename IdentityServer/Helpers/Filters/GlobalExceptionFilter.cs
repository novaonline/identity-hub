using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentityServer.Services;
using IdentityServer.Configuration;
using IdentityServ.Rules;
using IdentityServ.Models;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Microsoft.Extensions.Hosting;
using IdentityServer.Helpers.Migration;
using IdentityServer;
using IdentityServer.Helpers.Filters;

namespace IdentityServer.Helpers.Filters
{
	public class GlobalExceptionFilter : IExceptionFilter
	{
		private readonly ILogger _logger;

		public GlobalExceptionFilter(ILogger logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			_logger = logger;
		}

		public void OnException(ExceptionContext context)
		{
			_logger.LogError("GlobalExceptionFilter", context.Exception);
		}
	}
}
