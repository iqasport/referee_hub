using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum RefereeTeamAssociationType
{
	[EnumMember(Value = "player")]
	Player = 0,

	[EnumMember(Value = "coach")]
	Coach = 1,

	[EnumMember(Value = "national_team_player")]
	NationalTeamPlayer = 2,
}
