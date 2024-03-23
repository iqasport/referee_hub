using ManagementHub.Models.Domain.User;
using Microsoft.Extensions.Options.Contextual;

namespace ManagementHub.Service.Experimentation;

[OptionsContext]
public partial class ExperimentContext : IOptionsContext
{
	public ExperimentContext(UserIdentifier userId, ExperimentAssignmentContext assignmentContext)
	{
		this.UserId = userId;
		this.AssignmentContext = assignmentContext;
	}

	public UserIdentifier UserId { get; }

	public ExperimentAssignmentContext AssignmentContext { get; }
}

public enum ExperimentAssignmentContext
{
	None = 0,

	/// <summary>
	/// Experiment should be assigned on user sign in.
	/// </summary>
	SignIn = 1,

	/// <summary>
	/// Experiment should be assigned when running the adhoc job over eligible all users.
	/// </summary>
	AdHocJob = 2,
}
