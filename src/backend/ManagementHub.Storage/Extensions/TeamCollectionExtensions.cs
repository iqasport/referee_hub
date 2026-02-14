using System.Linq;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Storage.Extensions;

public static class TeamCollectionExtensions
{
	public static IQueryable<Team> WithIdentifier(this IQueryable<Team> teams, TeamIdentifier teamId)
		=> teams.Where(team => team.Id == teamId.Id);
}
