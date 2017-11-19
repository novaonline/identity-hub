using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Helpers
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

			this._logger = logger;
		}

		public void OnException(ExceptionContext context)
		{
			this._logger.LogError("GlobalExceptionFilter", context.Exception);
		}
	}
}
