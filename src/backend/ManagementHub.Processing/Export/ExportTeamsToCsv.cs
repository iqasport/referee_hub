using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using CsvHelper.Configuration.Attributes;
using ManagementHub.Models.Abstraction.Commands.Export;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Processing.Export;
public class ExportTeamsToCsv : IExportTeamsToCsv
{
	private readonly ILogger<ExportTeamsToCsv> logger;
	private readonly ITeamContextProvider teamContextProvider;
	private readonly INgbContextProvider ngbContextProvider;

	public ExportTeamsToCsv(ILogger<ExportTeamsToCsv> logger, ITeamContextProvider teamContextProvider, INgbContextProvider ngbContextProvider)
	{
		this.logger = logger;
		this.teamContextProvider = teamContextProvider;
		this.ngbContextProvider = ngbContextProvider;
	}

	public Stream ExportTeamsAsync(NgbConstraint ngbs, CancellationToken cancellationToken)
	{
		var ngbMap = this.ngbContextProvider.GetNgbContextsAsync(ngbs);
		var teams = this.teamContextProvider.GetTeams(ngbs);

		cancellationToken.ThrowIfCancellationRequested();

		return Join(teams, ngbMap, cancellationToken).ExportAsyncEnumerableAsCsv((pair) =>
		{
			var (team, ngb) = pair;
			return new CsvRow
			{
				Name = team.TeamData.Name,
				NationalGoverningBody = ngb.NgbData.Name,
				City = team.TeamData.City,
				GroupType = team.TeamData.GroupAffiliation.ToString(),
				State = team.TeamData.State ?? string.Empty,
				Status = team.TeamData.Status.ToString(),
			};
		}, cancellationToken, this.logger);
	}

	private static async IAsyncEnumerable<(ITeamContext, INgbContext)> Join(IEnumerable<ITeamContext> teams, IAsyncEnumerable<INgbContext> ngbs, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var ngbMap = await ngbs.ToDictionaryAsync(ngb => ngb.NgbId, cancellationToken);
		foreach (var team in teams)
		{
			if (ngbMap.TryGetValue(team.NgbId, out var ngb))
			{
				yield return (team, ngb);
			}
		}
	}

	private class CsvRow
	{
		public required string Name { get; set; }

		[Name("National Governing Body")]
		public required string NationalGoverningBody { get; set; }

		public required string City{ get; set; }

		[Name("State/Provence")]
		public required string State { get; set; }

		public required string Status { get; set; }

		[Name("Group Type")]
		public required string GroupType { get; set; }

	}
}
