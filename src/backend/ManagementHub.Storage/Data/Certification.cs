using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Data;

public partial class Certification : IIdentifiable
{
	public Certification()
	{
		this.CertificationPayments = new HashSet<CertificationPayment>();
		this.RefereeCertifications = new HashSet<RefereeCertification>();
		this.Tests = new HashSet<Test>();
	}

	public long Id { get; set; }
	public CertificationLevel Level { get; set; }
	public string DisplayName { get; set; } = null!;
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public CertificationVersion? Version { get; set; }

	public virtual ICollection<CertificationPayment> CertificationPayments { get; set; }
	public virtual ICollection<RefereeCertification> RefereeCertifications { get; set; }
	public virtual ICollection<Test> Tests { get; set; }
}
