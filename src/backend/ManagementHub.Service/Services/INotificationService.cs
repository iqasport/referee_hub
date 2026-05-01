using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Notification;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using NotificationEntity = ManagementHub.Models.Data.Notification;

namespace ManagementHub.Service.Services;

public interface INotificationService
{
	Task<NotificationEntity> CreateNgbAdminAssignmentNotificationAsync(
		UserIdentifier userId,
		NgbIdentifier ngb,
		CancellationToken cancellationToken = default);

	Task<NotificationEntity> CreateTeamManagerAssignmentNotificationAsync(
		UserIdentifier userId,
		TeamIdentifier teamId,
		CancellationToken cancellationToken = default);

	Task<NotificationEntity> CreateTournamentManagerAssignmentNotificationAsync(
		UserIdentifier userId,
		TournamentIdentifier tournamentId,
		string tournamentName,
		CancellationToken cancellationToken = default);

	Task<NotificationEntity> CreateTournamentInviteNotificationAsync(
		UserIdentifier userId,
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		string tournamentName,
		CancellationToken cancellationToken = default);

	Task<NotificationEntity> CreateTeamTournamentJoinRequestNotificationAsync(
		UserIdentifier userId,
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		string tournamentName,
		CancellationToken cancellationToken = default);

	Task<NotificationEntity> CreateRequestResponseNotificationAsync(
		UserIdentifier userId,
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		string tournamentName,
		bool approved,
		CancellationToken cancellationToken = default);

	Task<NotificationEntity> CreateInviteResponseNotificationAsync(
		UserIdentifier userId,
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		string tournamentName,
		bool approved,
		CancellationToken cancellationToken = default);

	Task<NotificationEntity> CreateRosterRegistrationNotificationAsync(
		UserIdentifier userId,
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		string tournamentName,
		RosterRole role,
		CancellationToken cancellationToken = default);

	Task<NotificationEntity> CreateExamResultNotificationAsync(
		UserIdentifier userId,
		TestIdentifier testId,
		string testTitle,
		Percentage score,
		bool passed,
		CancellationToken cancellationToken = default);

	Task<IEnumerable<NotificationEntity>> GetActiveNotificationsAsync(
		UserIdentifier userId,
		CancellationToken cancellationToken = default);

	Task<int> GetUnreadCountAsync(
		UserIdentifier userId,
		CancellationToken cancellationToken = default);

	Task<NotificationEntity?> MarkAsReadAsync(
		UserIdentifier userId,
		NotificationIdentifier notificationId,
		CancellationToken cancellationToken = default);

	Task<int> MarkAllAsReadAsync(
		UserIdentifier userId,
		CancellationToken cancellationToken = default);

	Task<bool> DeleteNotificationAsync(
		UserIdentifier userId,
		NotificationIdentifier notificationId,
		CancellationToken cancellationToken = default);

	Task ArchiveOldNotificationsAsync(
		CancellationToken cancellationToken = default);
}