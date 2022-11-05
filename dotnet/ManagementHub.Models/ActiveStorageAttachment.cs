using System;
using System.Collections.Generic;

namespace ManagementHub.Models
{
    public partial class ActiveStorageAttachment
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string RecordType { get; set; } = null!;
        public long RecordId { get; set; }
        public long BlobId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ActiveStorageBlob Blob { get; set; } = null!;
    }
}
