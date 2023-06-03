using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Data;

public partial class Answer : IIdentifiable
{
	public Answer()
	{
		this.RefereeAnswers = new HashSet<RefereeAnswer>();
	}

	public long Id { get; set; }
	public long QuestionId { get; set; }
	public string Description { get; set; } = null!;
	public bool Correct { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }

	public virtual Question Question { get; set; } = null!;
	public virtual ICollection<RefereeAnswer> RefereeAnswers { get; set; }
}
