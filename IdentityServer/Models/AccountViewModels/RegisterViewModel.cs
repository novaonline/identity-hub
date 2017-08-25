using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models.AccountViewModels
{
    public class RegisterViewModel
    {
		[Required]
		[Display(Name = "User Name")]
		[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Only Alphabets and Numbers allowed.")]
		public string UserName { get; set; }

		[Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

		[Display(Name = "Date of Birth")]
		[DataType(DataType.Date)]
		public DateTime BirthDate { get; set; }

		[DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

		[StringLength(50)]
		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		[StringLength(50)]
		[Display(Name = "Last Name")]
		public string LastName { get; set; }

		[Display(Name = "Profile URL")]
		[RegularExpression("^https?://", ErrorMessage = "Url must start with http or https")]
		public string ProfilePictureUrl { get; set; }
	}
}
