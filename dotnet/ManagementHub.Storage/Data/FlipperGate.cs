using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Data
{
	public partial class FlipperGate : IIdentifiable
	{
		public long Id { get; set; }
		public string FeatureKey { get; set; } = null!;
		public string Key { get; set; } = null!;
		public string? Value { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
