using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models
{
	public partial class RefereeLocation : IIdentifiable
	{
		public long Id { get; set; }
		public long RefereeId { get; set; }
		public long NationalGoverningBodyId { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public int? AssociationType { get; set; }

		public virtual NationalGoverningBody NationalGoverningBody { get; set; } = null!;
		public virtual User Referee { get; set; } = null!;
	}
}
