using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum ParticipantType
{
	[EnumMember(Value = "team")]
	Team = 0
}
