using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum UserAccessType
{
	[EnumMember(Value = "referee")]
	Referee = 0,

	[EnumMember(Value = "ngb_admin")]
	NgbAdmin = 1,

	[EnumMember(Value = "iqa_admin")]
	IqaAdmin = 2,
}
