using System.Linq;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Storage.Contexts.Team;

public record DbTeamContext(TeamIdentifier TeamId, NgbIdentifier NgbId, TeamData TeamData) : ITeamContext;

public class DbTeamContextFactory
{
	private readonly IQueryable<Models.Data.Team> teams;
	private readonly IQueryable<Models.Data.NationalGoverningBody> nationalGoverningBodies;

	public DbTeamContextFactory(IQueryable<Models.Data.Team> teams, IQueryable<NationalGoverningBody> nationalGoverningBodies)
	{
		this.teams = teams;
		this.nationalGoverningBodies = nationalGoverningBodies;
	}

	public IQueryable<ITeamContext> QueryTeams(NgbConstraint ngbs)
	{
		IQueryable<Models.Data.Team> t = this.teams.AsNoTracking()
				.Include(t => t.NationalGoverningBody);
		
		if (!ngbs.AppliesToAny)
		{
			t = t.Join(this.nationalGoverningBodies.WithConstraint(ngbs), tt => tt.NationalGoverningBodyId, n => n.Id, (tt, n) => tt);
		}

		return t.Select(tt => new DbTeamContext(new TeamIdentifier(tt.Id), new NgbIdentifier(tt.NationalGoverningBody.CountryCode), new TeamData
		{
			Name = tt.Name,
			City = tt.City,
			State = tt.State,
			Country = tt.Country,
			GroupAffiliation = tt.GroupAffiliation.Value,
			Status = tt.Status.Value,
		}));
	}
}
