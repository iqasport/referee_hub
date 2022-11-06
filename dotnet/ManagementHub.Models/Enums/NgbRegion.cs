using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums
{
	[JsonConverter(typeof(JsonStringEnumMemberConverter))]
	public enum NgbRegion
	{
		[EnumMember(Value = "north_america")]
		NorthAmerica = 0,

		[EnumMember(Value = "south_america")]
		SouthAmerica = 1,
		
		[EnumMember(Value = "europe")]
		Europe = 2,

		[EnumMember(Value = "africa")]
		Africa = 3,

		[EnumMember(Value = "asia")]
		Asia = 4,
	}
}
