using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Domain.Notification;

/// <summary>
/// Notification types for grouping and categorization.
/// </summary>
[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum NotificationType
{
	[EnumMember(Value = nameof(ExamResult))]
	ExamResult,
	[EnumMember(Value = nameof(TournamentInvite))]
	TournamentInvite,
	[EnumMember(Value = nameof(TeamTournamentJoinRequest))]
	TeamTournamentJoinRequest,
	[EnumMember(Value = nameof(ManagerAssignment))]
	ManagerAssignment,
	[EnumMember(Value = nameof(InviteAccepted))]
	InviteAccepted,
	[EnumMember(Value = nameof(InviteRejected))]
	InviteRejected,
	[EnumMember(Value = nameof(RequestAccepted))]
	RequestAccepted,
	[EnumMember(Value = nameof(RequestRejected))]
	RequestRejected,
	[EnumMember(Value = nameof(TeamApprovalNeeded))]
	TeamApprovalNeeded,
	[EnumMember(Value = nameof(NgbApprovalNeeded))]
	NgbApprovalNeeded,
	[EnumMember(Value = nameof(RosterRegistration))]
	RosterRegistration,
}
