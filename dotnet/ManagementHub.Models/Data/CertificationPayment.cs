using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Data
{
	public partial class CertificationPayment : IIdentifiable
	{
		public long Id { get; set; }
		public long UserId { get; set; }
		public long CertificationId { get; set; }
		public string StripeSessionId { get; set; } = null!;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public virtual Certification Certification { get; set; } = null!;
		public virtual User User { get; set; } = null!;
	}
}
