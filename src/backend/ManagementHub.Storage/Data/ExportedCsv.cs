using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Data;

public partial class ExportedCsv : IIdentifiable
{
	public long Id { get; set; }
	public string? Type { get; set; }
	public int UserId { get; set; }
	public string? Url { get; set; }
	public DateTime? ProcessedAt { get; set; }
	public DateTime? SentAt { get; set; }
	public string ExportOptions { get; set; } = null!;
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
}
