using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class SocialAccount
    {
        public long Id { get; set; }
        public string? OwnableType { get; set; }
        public long? OwnableId { get; set; }
        public string Url { get; set; } = null!;
        public int AccountType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
