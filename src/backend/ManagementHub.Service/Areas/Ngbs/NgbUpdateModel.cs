using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Enums;

namespace ManagementHub.Service.Areas.Ngbs;

public class NgbUpdateModel
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

	/// <summary>
	/// Website of the NGB.
	/// </summary>
	public Uri? Website { get; set; }

	/// <summary>
	/// Number of players as declared by the NGB.
	/// </summary>
	public int PlayerCount { get; set; }

	/// <summary>
	/// Social account URLs.
	/// </summary>
	public required IEnumerable<SocialAccount> SocialAccounts { get; set; }
}

public class AdminNgbUpdateModel : NgbUpdateModel
{
	/// <summary>
	/// Membership status of the NGB.
	/// </summary>
	public required NgbMembershipStatus MembershipStatus { get; set; }

	/// <summary>
	/// Region the NGB is in.
	/// </summary>
	public required NgbRegion? Region { get; set; }
}
