using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class NationalGoverningBodyAdmin
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long NationalGoverningBodyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual NationalGoverningBody NationalGoverningBody { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
