using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ManagementHub.Models.Abstraction.Commands.Export;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Processing.Export;
public class ExportRefereesToCsv : IExportRefereesToCsv
{
	private readonly IRefereeContextProvider refereeContextProvider;
	private readonly ILogger<ExportRefereesToCsv> logger;
	private readonly ITeamContextProvider teamContextProvider;

	public ExportRefereesToCsv(IRefereeContextProvider refereeContextProvider, ILogger<ExportRefereesToCsv> logger, ITeamContextProvider teamContextProvider)
	{
		this.refereeContextProvider = refereeContextProvider;
		this.logger = logger;
		this.teamContextProvider = teamContextProvider;
	}

	public Stream ExportRefereesAsync(NgbConstraint ngbs, CancellationToken cancellationToken)
	{
		var referees = this.refereeContextProvider.GetRefereeViewContextAsyncEnumerable(ngbs);
		var teams = this.teamContextProvider.GetTeams(NgbConstraint.Any).ToDictionary(t => t.TeamId, t => t.TeamData.Name);

		cancellationToken.ThrowIfCancellationRequested();

		return referees.ExportAsyncEnumerableAsCsv((referee) =>
		{
			var refereeTeams = new List<string>(2);
			if (referee.PlayingTeam.HasValue)
			{
				if (teams.TryGetValue(referee.PlayingTeam.Value, out var teamName))
					refereeTeams.Add(teamName);
				else
					this.logger.LogWarning(0x6caf5d00, "Referee {userId} has a playing team {teamId} that does not exist in the collection.", referee.UserId, referee.PlayingTeam.Value);
			}
			if (referee.CoachingTeam.HasValue)
			{
				if (teams.TryGetValue(referee.CoachingTeam.Value, out var teamName))
					refereeTeams.Add(teamName);
				else
					this.logger.LogWarning(0x6caf5d01, "Referee {userId} has a coaching team {teamId} that does not exist in the collection.", referee.UserId, referee.CoachingTeam.Value);
			}

			return new CsvRow
			{
				Name = referee.DisplayName,
				Certifications = string.Join(", ", referee.AcquiredCertifications.Select(c => c.Level).Distinct().Order()), // TODO: make this more useful to ngbs after consulting
				Teams = string.Join(", ", refereeTeams),
			};
		}, cancellationToken, this.logger);
	}

	private class CsvRow
	{
		public required string Name { get; set; }
		public required string Teams { get; set; }
		public required string Certifications { get; set; }
	}
}
