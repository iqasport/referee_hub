using System;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models
{
	public partial class SocialAccount : IIdentifiable
	{
		public long Id { get; set; }
		public string? OwnableType { get; set; }
		public long? OwnableId { get; set; }
		public string Url { get; set; } = null!;
		public SocialAccountType AccountType { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
