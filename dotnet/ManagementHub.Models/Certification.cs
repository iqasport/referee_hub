using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models
{
	public partial class Certification : IIdentifiable
	{
		public Certification()
		{
			CertificationPayments = new HashSet<CertificationPayment>();
			RefereeCertifications = new HashSet<RefereeCertification>();
			Tests = new HashSet<Test>();
		}

		public long Id { get; set; }
		public TestLevel Level { get; set; }
		public string DisplayName { get; set; } = null!;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public CertificationVersion? Version { get; set; }

		public virtual ICollection<CertificationPayment> CertificationPayments { get; set; }
		public virtual ICollection<RefereeCertification> RefereeCertifications { get; set; }
		public virtual ICollection<Test> Tests { get; set; }
	}
}
