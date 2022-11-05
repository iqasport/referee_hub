using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class Certification
    {
        public Certification()
        {
            CertificationPayments = new HashSet<CertificationPayment>();
            RefereeCertifications = new HashSet<RefereeCertification>();
            Tests = new HashSet<Test>();
        }

        public long Id { get; set; }
        public long Level { get; set; }
        public string DisplayName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Version { get; set; }

        public virtual ICollection<CertificationPayment> CertificationPayments { get; set; }
        public virtual ICollection<RefereeCertification> RefereeCertifications { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
    }
}
