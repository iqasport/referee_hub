using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums
{
	[JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum TeamGroupAffiliation
	{
		[EnumMember(Value = "university")]
		University = 0,
		
		[EnumMember(Value = "community")]
		Community = 1,
		
		[EnumMember(Value = "youth")]
		Youth = 2,
		
		[EnumMember(Value = "not_applicable")]
		NotApplicable = 3,
    }
}
