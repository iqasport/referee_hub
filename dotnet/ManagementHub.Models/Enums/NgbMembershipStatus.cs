using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums
{
	[JsonConverter(typeof(JsonStringEnumMemberConverter))]
	public enum NgbMembershipStatus
	{
		[EnumMember(Value = "area_of_interest")]
		AreaOfInterest = 0,

		[EnumMember(Value = "emerging")]
		Emerging = 1,

		[EnumMember(Value = "developing")]
		Developing = 2,

		[EnumMember(Value = "full")]
		Full = 3,
	}
}
