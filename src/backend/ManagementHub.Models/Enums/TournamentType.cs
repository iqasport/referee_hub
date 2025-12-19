using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TournamentType
{
	[EnumMember(Value = "club")]
	Club = 0,
	
	[EnumMember(Value = "national")]
	National = 1,
	
	[EnumMember(Value = "youth")]
	Youth = 2,
	
	[EnumMember(Value = "fantasy")]
	Fantasy = 3
}
