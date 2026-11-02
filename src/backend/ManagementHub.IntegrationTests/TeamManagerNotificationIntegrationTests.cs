using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using ManagementHub.Models.Domain.Notification;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Service.Services;
using ManagementHub.Storage;
using ManagementHub.Storage.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using NotificationEntity = ManagementHub.Models.Data.Notification;

namespace ManagementHub.IntegrationTests;

public class TeamManagerNotificationIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory factory;

	public TeamManagerNotificationIntegrationTests(TestWebApplicationFactory factory)
	{
		this.factory = factory;
	}

	[Theory]
	[InlineData("Yankees")]
	[InlineData("BA Jacks")]
	public async Task CreateTeamManagerNotification_ShouldPersistAndReturnTeamName(string teamName)
	{
		using var scope = this.factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ManagementHubDbContext>();
		await using var transaction = await db.Database.BeginTransactionAsync();
		var user = await db.Users.SingleAsync(item => item.Email == "referee@example.com");
		var userId = user.UniqueId != null ? UserIdentifier.Parse(user.UniqueId) : UserIdentifier.FromLegacyUserId(user.Id);
		var team = await db.Teams.SingleAsync(item => item.Name == teamName);
		var teamId = new TeamIdentifier(team.Id);
		var service = scope.ServiceProvider.GetRequiredService<INotificationService>();

		var notification = await service.CreateTeamManagerAssignmentNotificationAsync(userId, teamId);
		notification.Message.Should().Be($"You can now manage team {teamName}.");
		notification.RelatedEntityId.Should().Be(teamId.ToString());
		notification.RelatedEntityType.Should().Be("Team");
		notification.Type.Should().Be(NotificationType.ManagerAssignment);

		db.ChangeTracker.Clear();
		var stored = await db.Notifications.SingleAsync(item => item.Id == notification.Id);
		stored.Message.Should().Be(notification.Message);
		var active = await service.GetActiveNotificationsAsync(userId);
		active.Single(item => item.Id.ToString() == notification.UniqueId).Message.Should().Be(notification.Message);
	}

	[Fact]
	public async Task CreateTeamManagerNotification_MissingTeam_ShouldNotSaveNotification()
	{
		using var scope = this.factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ManagementHubDbContext>();
		await using var transaction = await db.Database.BeginTransactionAsync();
		var user = await db.Users.SingleAsync(item => item.Email == "referee@example.com");
		var userId = user.UniqueId != null ? UserIdentifier.Parse(user.UniqueId) : UserIdentifier.FromLegacyUserId(user.Id);
		var service = scope.ServiceProvider.GetRequiredService<INotificationService>();
		var before = await db.Notifications.CountAsync();

		Func<Task> create = () => service.CreateTeamManagerAssignmentNotificationAsync(userId, new TeamIdentifier(long.MaxValue));
		await create.Should().ThrowAsync<InvalidOperationException>();
		(await db.Notifications.CountAsync()).Should().Be(before);
	}

	[Theory]
	[InlineData(false, false)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public async Task Backfill_ShouldCorrectOnlyLegacyTeamMessagesAndPreserveState(bool read, bool archived)
	{
		using var scope = this.factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ManagementHubDbContext>();
		await using var transaction = await db.Database.BeginTransactionAsync();
		var user = await db.Users.SingleAsync(item => item.Email == "referee@example.com");
		var team = await db.Teams.SingleAsync(item => item.Name == "Yankees");
		var teamId = new TeamIdentifier(team.Id).ToString();
		var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		var oldMessage = $"You can now manage team {teamId}.";
		var notifications = new[]
		{
			(NotificationType.ManagerAssignment, "Team", teamId, oldMessage),
			(NotificationType.ManagerAssignment, "Team", teamId, "Custom assignment message"),
			(NotificationType.ManagerAssignment, "Team", teamId, $"You can now manage team {team.Name}."),
			(NotificationType.ManagerAssignment, "Ngb", teamId, oldMessage),
			(NotificationType.TeamApprovalNeeded, "Team", teamId, oldMessage),
			(NotificationType.ManagerAssignment, "Team", "TM_9223372036854775807", "You can now manage team TM_9223372036854775807."),
		}.Select(item => new NotificationEntity
		{
			UserId = user.Id,
			UniqueId = NotificationIdentifier.NewNotificationId().ToString(),
			Type = item.Item1,
			Title = "You were added as team manager",
			RelatedEntityType = item.Item2,
			RelatedEntityId = item.Item3,
			Message = item.Item4,
			CreatedAt = date,
			ReadAt = read ? date : null,
			ArchivedAt = archived ? date : null,
		}).ToArray();
		db.Notifications.AddRange(notifications);
		await db.SaveChangesAsync();

		db.Database.GetMigrations().Should().Contain("20261102161500_UseTeamNamesInManagerNotifications");
		var migration = new UseTeamNamesInManagerNotifications();
		foreach (var operation in migration.UpOperations.Cast<SqlOperation>())
		{
			await db.Database.ExecuteSqlRawAsync(operation.Sql);
		}

		db.ChangeTracker.Clear();
		for (var index = 0; index < notifications.Length; index++)
		{
			var original = notifications[index];
			var updated = await db.Notifications.SingleAsync(item => item.Id == original.Id);
			updated.Message.Should().Be(index == 0 ? $"You can now manage team {team.Name}." : original.Message);
			updated.UniqueId.Should().Be(original.UniqueId);
			updated.CreatedAt.Should().Be(original.CreatedAt);
			updated.ReadAt.Should().Be(original.ReadAt);
			updated.ArchivedAt.Should().Be(original.ArchivedAt);
			updated.RelatedEntityId.Should().Be(original.RelatedEntityId);
		}
	}
}
