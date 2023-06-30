using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Data;

public partial class NationalGoverningBody : IIdentifiable
{
	public NationalGoverningBody()
	{
		this.NationalGoverningBodyAdmins = new HashSet<NationalGoverningBodyAdmin>();
		this.NationalGoverningBodyStats = new HashSet<NationalGoverningBodyStat>();
		this.RefereeLocations = new HashSet<RefereeLocation>();
		this.Teams = new HashSet<Team>();
	}

	public long Id { get; set; }
	public string CountryCode { get; set; } = null!; // new identifier
	public string Name { get; set; } = null!;
	public string? Website { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public int PlayerCount { get; set; }
	public string? ImageUrl { get; set; }
	public string? Country { get; set; }
	public string? Acronym { get; set; }
	public NgbRegion? Region { get; set; }
	public NgbMembershipStatus MembershipStatus { get; set; }

	public virtual ICollection<NationalGoverningBodyAdmin> NationalGoverningBodyAdmins { get; set; }
	public virtual ICollection<NationalGoverningBodyStat> NationalGoverningBodyStats { get; set; }
	public virtual ICollection<RefereeLocation> RefereeLocations { get; set; }
	public virtual ICollection<Team> Teams { get; set; }
}
