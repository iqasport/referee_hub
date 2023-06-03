using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.Ngb;

public class NgbData
{
	public NgbData(string name)
	{
		this.Name = name;
	}

	/// <summary>
	/// Official name of the NGB. 
	/// </summary>
	public string Name { get; set; }

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
}
