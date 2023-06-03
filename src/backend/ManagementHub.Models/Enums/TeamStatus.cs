using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TeamStatus
{
	[EnumMember(Value = "competitive")]
	Competitive = 0,

	[EnumMember(Value = "developing")]
	Developing = 1,

	[EnumMember(Value = "inactive")]
	Inactive = 2,

	[EnumMember(Value = "other")]
	Other = 3,
}
