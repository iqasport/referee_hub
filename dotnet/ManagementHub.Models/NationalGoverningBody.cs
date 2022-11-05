using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class NationalGoverningBody
    {
        public NationalGoverningBody()
        {
            NationalGoverningBodyAdmins = new HashSet<NationalGoverningBodyAdmin>();
            NationalGoverningBodyStats = new HashSet<NationalGoverningBodyStat>();
            RefereeLocations = new HashSet<RefereeLocation>();
            Teams = new HashSet<Team>();
        }

        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Website { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int PlayerCount { get; set; }
        public string? ImageUrl { get; set; }
        public string? Country { get; set; }
        public string? Acronym { get; set; }
        public int? Region { get; set; }
        public int MembershipStatus { get; set; }

        public virtual ICollection<NationalGoverningBodyAdmin> NationalGoverningBodyAdmins { get; set; }
        public virtual ICollection<NationalGoverningBodyStat> NationalGoverningBodyStats { get; set; }
        public virtual ICollection<RefereeLocation> RefereeLocations { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
    }
}
