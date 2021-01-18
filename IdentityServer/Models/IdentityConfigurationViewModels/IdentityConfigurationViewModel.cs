using IdentityServer4.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models.IdentityConfigurationViewModels
{
	public class IdentityConfigurationViewModel
	{
		public AddClientViewModel AddClientViewModel { get; set; }
		public IEnumerable<Client> Clients { get; set; }
		public IEnumerable<ResponseAfterCreate> ResponsesAfterCreate { get; set; }
	}
}
