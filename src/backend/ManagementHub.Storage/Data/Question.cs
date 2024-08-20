using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Data;

public partial class Question : IIdentifiable
{
	public Question()
	{
		this.Answers = new HashSet<Answer>();
		this.RefereeAnswers = new HashSet<RefereeAnswer>();
	}

	public long Id { get; set; }
	public long TestId { get; set; }
	public required long SequenceId { get; set; } // sequence number relevant in import/export for idempotency
	public required string Description { get; set; } = null!;
	public required int PointsAvailable { get; set; }
	public string? Feedback { get; set; }
	public DateTime CreatedAt { get; set; }
	public required DateTime UpdatedAt { get; set; }

	public virtual Test Test { get; set; } = null!;
	public virtual ICollection<Answer> Answers { get; set; }
	public virtual ICollection<RefereeAnswer> RefereeAnswers { get; set; }
}
