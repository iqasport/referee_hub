using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class Language
    {
        public Language()
        {
            Tests = new HashSet<Test>();
            Users = new HashSet<User>();
        }

        public long Id { get; set; }
        public string LongName { get; set; } = null!;
        public string ShortName { get; set; } = null!;
        public string? LongRegion { get; set; }
        public string? ShortRegion { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Test> Tests { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
