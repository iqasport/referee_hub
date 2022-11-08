using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums
{
	[JsonConverter(typeof(JsonStringEnumMemberConverter))]
	public enum TestLevel
	{
		[EnumMember(Value = "snitch")]
		Snitch = 0,

		[EnumMember(Value = "assistant")]
		Assistant = 1,

		[EnumMember(Value = "head")]
		Head = 2,

		[EnumMember(Value = "scorekeeper")]
		Scorekeeper = 3,
	}
}
