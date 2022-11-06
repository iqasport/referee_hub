using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models
{
	public partial class Team : IIdentifiable
	{
		public Team()
		{
			RefereeTeams = new HashSet<RefereeTeam>();
			TeamStatusChangesets = new HashSet<TeamStatusChangeset>();
		}

		public long Id { get; set; }
		public string Name { get; set; } = null!;
		public string City { get; set; } = null!;
		public string? State { get; set; }
		public string Country { get; set; } = null!;
		public int? Status { get; set; }
		public int? GroupAffiliation { get; set; }
		public long? NationalGoverningBodyId { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public DateTime? JoinedAt { get; set; }

		public virtual NationalGoverningBody? NationalGoverningBody { get; set; }
		public virtual ICollection<RefereeTeam> RefereeTeams { get; set; }
		public virtual ICollection<TeamStatusChangeset> TeamStatusChangesets { get; set; }
	}
}
