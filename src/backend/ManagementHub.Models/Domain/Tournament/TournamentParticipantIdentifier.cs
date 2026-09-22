using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.Tournament;

public readonly record struct TournamentParticipantIdentifier
{
	private TournamentParticipantIdentifier(ParticipantType participantType, TeamIdentifier? teamId, UserIdentifier? userId)
	{
		this.ParticipantType = participantType;
		this.TeamId = teamId;
		this.UserId = userId;
	}

	public ParticipantType ParticipantType { get; }

	public TeamIdentifier? TeamId { get; }

	public UserIdentifier? UserId { get; }

	public static TournamentParticipantIdentifier ForTeam(TeamIdentifier teamId)
		=> new(ParticipantType.Team, teamId, null);

	public static TournamentParticipantIdentifier ForReferee(UserIdentifier userId)
		=> new(ParticipantType.Referee, null, userId);

	public static bool TryParse(ParticipantType participantType, string value, out TournamentParticipantIdentifier participantIdentifier)
	{
		participantIdentifier = default;

		if (participantType == ParticipantType.Team && TeamIdentifier.TryParse(value, out var teamId))
		{
			participantIdentifier = ForTeam(teamId);
			return true;
		}

		if (participantType == ParticipantType.Referee && UserIdentifier.TryParse(value, out var userId))
		{
			participantIdentifier = ForReferee(userId);
			return true;
		}

		return false;
	}

	public static bool TryParse(string value, out TournamentParticipantIdentifier participantIdentifier)
	{
		participantIdentifier = default;

		if (TeamIdentifier.TryParse(value, out var teamId))
		{
			participantIdentifier = ForTeam(teamId);
			return true;
		}

		if (UserIdentifier.TryParse(value, out var userId))
		{
			participantIdentifier = ForReferee(userId);
			return true;
		}

		return false;
	}

	public override string ToString()
		=> this.ParticipantType == ParticipantType.Team
			? this.TeamId?.ToString() ?? string.Empty
			: this.UserId?.ToString() ?? string.Empty;
}