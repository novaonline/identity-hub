using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models.AccountViewModels
{
    public class ChangeProfileViewModel
    {
		[StringLength(50)]
		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		[StringLength(50)]
		[Display(Name = "Last Name")]
		public string LastName { get; set; }
	}
}
