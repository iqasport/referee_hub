using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums;

// When adding new enum
// Run the following on the database
//   insert into certifications (level, display_name, created_at, updated_at, version)
//   select level, display_name, now() as created_at, now() as updated_at, <NEW_VERSION_NUM> as version
//   from certifications    group by level, display_name

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum CertificationVersion
{
	[EnumMember(Value = "eighteen")]
	Eighteen = 0,

	[EnumMember(Value = "twenty")]
	Twenty = 1,

	[EnumMember(Value = "twentytwo")]
	TwentyTwo = 2,

	[EnumMember(Value = "twentyfour")]
	TwentyFour = 3,
}
