using System;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.Ngb;

public class NgbData
{
	/// <summary>
	/// Official name of the NGB. 
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Country name where NGB is located.
	/// </summary>
	public string? Country { get; set; }

	/// <summary>
	/// 3 letter country acronym.
	/// </summary>
	public string? Acronym { get; set; }

	public NgbRegion? Region { get; set; }

	public NgbMembershipStatus MembershipStatus { get; set; }

	public Uri? Website { get; set; }
}
