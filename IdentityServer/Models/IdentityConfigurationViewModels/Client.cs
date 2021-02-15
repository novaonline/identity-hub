using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models.IdentityConfigurationViewModels
{
	/// <summary>
	/// Id is derived from the Client name
	/// </summary>
	public class AddClientViewModel
	{
		[Display(Name = "Client Name")]
		public string ClientName { get; set; }

		[Display(Name = "Grant Type")]
		public GrantType GrantType { get; set; }

		/// <summary>
		/// The API will automatically create API scopes {ApiName}.read, {ApiName}.write, {ApiName}.delete.
		/// Additionally It will create a "manage" scope
		/// Anything in bracked its added to the description
		/// </summary>
		[Display(Name = "Api Resources (comma seperated)")]
		public string AllowedApiResourcesCommaSeperated { get; set; }

		[Display(Name = "Allow Access Token via Browser")]
		public bool AllowAccessTokenViaBrowser { get; set; }
		
		[Display(Name = "Require Consent")]
		public bool RequireConsent { get; set; }
		
		[Display(Name = "Require Secret")]
		public bool RequireSecret { get; set; }
		
		[Display(Name = "Include Profile Scope")]
		public bool IncludeProfile { get; set; }

		[Display(Name = "Allow Admin Scope")]
		public bool AllowAdminScope { get; set; }

		[Display(Name = "Base Url")]
		public Uri BaseUrl { get; set; }
		
		[Display(Name = "Redirect Path")]
		public string RedirectPath { get; set; }
		
		[Display(Name = "Logout Path")]
		public string LogoutPath { get; set; }
	}
}
