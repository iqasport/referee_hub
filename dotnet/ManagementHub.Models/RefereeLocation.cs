using System;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models
{
	public partial class RefereeLocation : IIdentifiable
	{
		public long Id { get; set; }
		public long RefereeId { get; set; }
		public long NationalGoverningBodyId { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public RefereeNgbAssociationType? AssociationType { get; set; }

		public virtual NationalGoverningBody NationalGoverningBody { get; set; } = null!;
		public virtual User Referee { get; set; } = null!;
	}
}
