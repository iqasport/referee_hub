using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models
{
	public partial class FlipperFeature : IIdentifiable
	{
		public long Id { get; set; }
		public string Key { get; set; } = null!;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
