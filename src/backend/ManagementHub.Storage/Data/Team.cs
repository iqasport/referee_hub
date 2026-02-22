using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Data;

public partial class Team : IIdentifiable
{
	public Team()
	{
		this.RefereeTeams = new HashSet<RefereeTeam>();
		this.TeamStatusChangesets = new HashSet<TeamStatusChangeset>();
		this.TeamManagers = new HashSet<TeamManager>();
		this.TournamentTeamParticipants = new HashSet<TournamentTeamParticipant>();
	}

	public long Id { get; set; }
	public string Name { get; set; } = null!;
	public string City { get; set; } = null!;
	public string? State { get; set; }
	public string Country { get; set; } = null!;
	public TeamStatus? Status { get; set; }
	public TeamGroupAffiliation? GroupAffiliation { get; set; }
	public long? NationalGoverningBodyId { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public DateTime? JoinedAt { get; set; }
	public string? LogoUrl { get; set; }
	public string? Description { get; set; }
	public string? ContactEmail { get; set; }

	public virtual NationalGoverningBody? NationalGoverningBody { get; set; }
	public virtual ICollection<RefereeTeam> RefereeTeams { get; set; }
	public virtual ICollection<TeamStatusChangeset> TeamStatusChangesets { get; set; }
	public virtual ICollection<TeamManager> TeamManagers { get; set; }
	public virtual ICollection<TournamentTeamParticipant> TournamentTeamParticipants { get; set; }
}
