using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class RefereeLocation
    {
        public long Id { get; set; }
        public long RefereeId { get; set; }
        public long NationalGoverningBodyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? AssociationType { get; set; }

        public virtual NationalGoverningBody NationalGoverningBody { get; set; } = null!;
        public virtual User Referee { get; set; } = null!;
    }
}
