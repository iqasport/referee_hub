using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Data;

public partial class Tournament : IIdentifiable
{
	public Tournament()
	{
		this.TournamentManagers = new HashSet<TournamentManager>();
		this.TournamentInvites = new HashSet<TournamentInvite>();
		this.TournamentTeamParticipants = new HashSet<TournamentTeamParticipant>();
	}

	public long Id { get; set; }
	public string UniqueId { get; set; } = null!;
	public string Name { get; set; } = null!;
	public string Description { get; set; } = null!;
	public DateOnly StartDate { get; set; }
	public DateOnly EndDate { get; set; }
	public TournamentType Type { get; set; }
	public string Country { get; set; } = null!;
	public string City { get; set; } = null!;
	public string? Place { get; set; }
	public string Organizer { get; set; } = null!;
	public bool IsPrivate { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }

	public virtual ICollection<TournamentManager> TournamentManagers { get; set; }
	public virtual ICollection<TournamentInvite> TournamentInvites { get; set; }
	public virtual ICollection<TournamentTeamParticipant> TournamentTeamParticipants { get; set; }
}
