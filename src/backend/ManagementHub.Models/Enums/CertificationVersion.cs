using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum CertificationVersion
{
	[EnumMember(Value = "eighteen")]
	Eighteen = 0,

	[EnumMember(Value = "twenty")]
	Twenty = 1,

	[EnumMember(Value = "twentytwo")]
	TwentyTwo = 2,
}
