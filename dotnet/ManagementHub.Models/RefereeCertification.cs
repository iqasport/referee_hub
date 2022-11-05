using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class RefereeCertification
    {
        public long Id { get; set; }
        public long RefereeId { get; set; }
        public long CertificationId { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime? RenewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? NeedsRenewalAt { get; set; }

        public virtual Certification Certification { get; set; } = null!;
        public virtual User Referee { get; set; } = null!;
    }
}
