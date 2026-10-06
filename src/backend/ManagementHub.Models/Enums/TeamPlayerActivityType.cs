using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TeamPlayerActivityType
{
	[EnumMember(Value = "inviteCreated")]
	InviteCreated = 0,
	[EnumMember(Value = "inviteRevoked")]
	InviteRevoked = 1,
	[EnumMember(Value = "inviteAccepted")]
	InviteAccepted = 2,
	[EnumMember(Value = "inviteDeclined")]
	InviteDeclined = 3,
	[EnumMember(Value = "playerRemoved")]
	PlayerRemoved = 4,
}