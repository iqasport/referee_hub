using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Domain.Tests;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TestAttemptFinishMethod : byte
{
	[EnumMember(Value = nameof(Timeout))]
	Timeout = 0,

	[EnumMember(Value = nameof(Submission))]
	Submission = 1,
}
