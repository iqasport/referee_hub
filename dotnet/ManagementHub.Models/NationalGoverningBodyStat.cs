using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models
{
	public partial class NationalGoverningBodyStat : IIdentifiable
	{
		public long Id { get; set; }
		public long? NationalGoverningBodyId { get; set; }
		public int? TotalRefereesCount { get; set; }
		public int? HeadRefereesCount { get; set; }
		public int? AssistantRefereesCount { get; set; }
		public int? SnitchRefereesCount { get; set; }
		public int? CompetitiveTeamsCount { get; set; }
		public int? DevelopingTeamsCount { get; set; }
		public int? InactiveTeamsCount { get; set; }
		public int? YouthTeamsCount { get; set; }
		public int? UniversityTeamsCount { get; set; }
		public int? CommunityTeamsCount { get; set; }
		public int? TeamStatusChangeCount { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public int? TotalTeamsCount { get; set; }
		public int? UncertifiedCount { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? EndTime { get; set; }
		public int? ScorekeeperRefereesCount { get; set; }

		public virtual NationalGoverningBody? NationalGoverningBody { get; set; }
	}
}
