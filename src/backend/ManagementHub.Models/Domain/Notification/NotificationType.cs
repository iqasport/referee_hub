namespace ManagementHub.Models.Domain.Notification;

/// <summary>
/// Notification types for grouping and categorization.
/// </summary>
public enum NotificationType
{
	ExamResult,
	TournamentInvite,
	TeamTournamentJoinRequest,
	ManagerAssignment,
	InviteAccepted,
	InviteRejected,
	RequestAccepted,
	RequestRejected,
	TeamApprovalNeeded,
	NgbApprovalNeeded,
	RosterRegistration,
}
