using ManagementHub.Models.Enums;

namespace ManagementHub.Service.Areas.Ngbs;

public class NgbViewModel
{
	/// <summary>
	/// The identifier of the NGB.
	/// </summary>
	public required string CountryCode { get; set; }

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

	/// <summary>
	/// Region the NGB is in.
	/// </summary>
	public NgbRegion? Region { get; set; }

	/// <summary>
	/// Membership status of the NGB.
	/// </summary>
	public NgbMembershipStatus MembershipStatus { get; set; }
}
