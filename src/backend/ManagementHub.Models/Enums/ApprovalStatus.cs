using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum ApprovalStatus
{
	[EnumMember(Value = "pending")]
	Pending = 0,

	[EnumMember(Value = "approved")]
	Approved = 1,

	[EnumMember(Value = "rejected")]
	Rejected = 2
}
