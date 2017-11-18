using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace IdentityServ.Models
{
	// Add profile data for application users by adding properties to the ApplicationUser class
	// TODO add location information
	public class ApplicationUser : IdentityUser
	{
		[StringLength(50)]
		public string FirstName { get; set; }

		[StringLength(50)]
		public string LastName { get; set; }

		public DateTime BirthDate { get; set; }

		[RegularExpression("^https?://", ErrorMessage = "Url must start with http or https")]
		public string ProfilePictureUrl { get; set; }
	}
}
