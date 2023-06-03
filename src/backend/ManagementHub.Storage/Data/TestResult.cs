using System;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Data;

public partial class TestResult : IIdentifiable
{
	public long Id { get; set; }
	public string? UniqueId { get; set; }
	public long RefereeId { get; set; }
	public TimeOnly? TimeStarted { get; set; }
	public TimeOnly? TimeFinished { get; set; }
	public string? Duration { get; set; }
	public int? Percentage { get; set; }
	public int? PointsScored { get; set; }
	public int? PointsAvailable { get; set; }
	public bool? Passed { get; set; }
	public string? CertificateUrl { get; set; }
	public int? MinimumPassPercentage { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public TestLevel? TestLevel { get; set; }
	public long? TestId { get; set; }

	public virtual User Referee { get; set; } = null!;
	public virtual Test? Test { get; set; }
}
