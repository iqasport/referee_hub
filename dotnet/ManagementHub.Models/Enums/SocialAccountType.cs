using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums
{
	[JsonConverter(typeof(JsonStringEnumMemberConverter))]
	public enum SocialAccountType
	{
		[EnumMember(Value = "facebook")]
		Facebook = 0,

		[EnumMember(Value = "twitter")]
		Twitter = 1,

		[EnumMember(Value = "youtube")]
		YouTube = 2,

		[EnumMember(Value = "instagram")]
		Instagram = 3,

		[EnumMember(Value = "other")]
		Other = 4,
	}
}
