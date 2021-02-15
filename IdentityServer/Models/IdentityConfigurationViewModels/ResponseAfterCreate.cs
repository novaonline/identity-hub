using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models.IdentityConfigurationViewModels
{
	public class ResponseAfterCreate
	{
		[Display(Name = "Api Scope Name")]
		public string ApiScopeName { get; set; }
		public string Secret { get; set; }
		public string SecretPlainText { get; set; }
	}
}
