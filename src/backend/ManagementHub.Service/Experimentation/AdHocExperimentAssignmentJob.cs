using ManagementHub.Models.Domain.User;
using ManagementHub.Service.Contexts;
using ManagementHub.Storage;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Service.Experimentation;

public class AdHocExperimentAssignmentJob
{
	private readonly UxExperimentVariantAssigner experimentAssigner;
	// CHEAT - I don't feel like creating a whole new interface to write a quick query for userIds
	private readonly ManagementHubDbContext dbContext;

	public AdHocExperimentAssignmentJob(UxExperimentVariantAssigner experimentAssigner, ManagementHubDbContext dbContext)
	{
		this.experimentAssigner = experimentAssigner;
		this.dbContext = dbContext;
	}

	public async Task AssignExperimentsToAllEligibleUsersAsync(CancellationToken cancellationToken)
	{
		var eligibleUserIds = await this.dbContext.Users.AsNoTracking()
			.Where(u => u.UniqueId != null)
			.Select(u => UserIdentifier.Parse(u.UniqueId!))
			.ToListAsync(cancellationToken);

		foreach (var userId in eligibleUserIds)
		{
			await this.experimentAssigner.SaveUxExperimentAssignmentsAsync(
				new ExperimentContext(userId, ExperimentAssignmentContext.AdHocJob), cancellationToken);
		}
	}
}
