using System.Text.Json.Serialization;

namespace ManagementHub.Processing.Domain.Tests.Policies.Eligibility;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum RefereeEligibilityResult
{
	Unknown = 0, // default value shouldn't indicate anything
	Eligible = 1,

	MissingRequiredCertification = 2,
	RecertificationForLowerThanPreviouslyHeld = 3,
	RecertificationNotAllowedDueToInitialCertificationStarted = 4,
	TestAttemptedMaximumNumberOfTimes = 5,
	MissingCertificationPayment = 6,
	InCooldownPeriod = 7,
	RefereeAlreadyCertified = 8,
}
