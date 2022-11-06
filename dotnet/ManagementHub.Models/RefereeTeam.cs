using System;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models
{
	public partial class RefereeTeam : IIdentifiable
	{
		public long Id { get; set; }
		public long? TeamId { get; set; }
		public long? RefereeId { get; set; }
		public RefereeTeamAssociationType? AssociationType { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public virtual User? Referee { get; set; }
		public virtual Team? Team { get; set; }
	}
}
