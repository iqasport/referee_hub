using System;
using ManagementHub.Models.Data;

namespace ManagementHub.Models.Web
{
	/// <summary>
	/// View of <see cref="User"/> returned from the API.
	/// </summary>
	public class WebUser : User
	{
		public string? AvatarUrl { get; set; }
		public string[] EnabledFeatures { get; set; } = Array.Empty<string>();
		public bool HasPendingPolicies { get; set; }
	}
}
