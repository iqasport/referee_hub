using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums
{
	[JsonConverter(typeof(JsonStringEnumMemberConverter))]
	public enum RefereeNgbAssociationType
	{
		[EnumMember(Value = "primary")]
		Primary = 0,

		[EnumMember(Value = "secondary")]
		Secondary = 1,

		[EnumMember(Value = "other")]
		Other = 2,
	}
}
