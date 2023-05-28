using System.Collections.Generic;
using System.IO;
using System.Threading;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using Microsoft.Extensions.Logging;

public class ExportRefereesToCsv : IExportRefereesToCsv
{
	private readonly IRefereeContextProvider refereeContextProvider;
	private readonly ILogger<ExportRefereesToCsv> logger;

	public ExportRefereesToCsv(IRefereeContextProvider refereeContextProvider, ILogger<ExportRefereesToCsv> logger)
	{
		this.refereeContextProvider = refereeContextProvider;
		this.logger = logger;
	}

	public Stream ExportRefereesAsync(NgbConstraint ngbs, CancellationToken cancellationToken)
	{
		var referees = this.refereeContextProvider.GetRefereeViewContextAsyncEnumerable(ngbs);
		var teams = new Dictionary<TeamIdentifier, string>(); // TODO: load teams to get their names

		cancellationToken.ThrowIfCancellationRequested();

		return referees.ExportAsyncEnumerableAsCsv((referee) =>
		{
			var refereeTeams = new List<string>(2);
			if (referee.PlayingTeam.HasValue) refereeTeams.Add(teams[referee.PlayingTeam.Value]);
			if (referee.CoachingTeam.HasValue) refereeTeams.Add(teams[referee.CoachingTeam.Value]);

			return new CsvRow
			{
				Name = referee.DisplayName,
				Certifications = "", // TODO: format certifications
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
