using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums
{
	[JsonConverter(typeof(JsonStringEnumMemberConverter))]
	public enum CertificationLevel
	{
		[EnumMember(Value = "snitch")]
		Snitch = 0,

		Flag = 0,

		[EnumMember(Value = "assistant")]
		Assistant = 1,

		[EnumMember(Value = "head")]
		Head = 2,

		[EnumMember(Value = "field")]
		Field = 3,

		[EnumMember(Value = "scorekeeper")]
		Scorekeeper = 4,
	}
}
