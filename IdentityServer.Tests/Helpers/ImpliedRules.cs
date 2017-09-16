using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer.Tests.Helpers
{
    class ImpliedRules
    {
		public static bool ViewIsActionName(string viewName)
		{
			return String.IsNullOrEmpty(viewName);
		}
    }
}
