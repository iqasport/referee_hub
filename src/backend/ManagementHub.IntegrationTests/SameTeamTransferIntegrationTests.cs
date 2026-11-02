using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.IntegrationTests.Helpers;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using ManagementHub.Storage;
using ManagementHub.Storage.Commands.Team;
using ManagementHub.Storage.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ManagementHub.IntegrationTests;

public class SameTeamTransferIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
	private readonly TestWebApplicationFactory factory;

	public SameTeamTransferIntegrationTests(TestWebApplicationFactory factory)
	{
		this.factory = factory;
	}

	[Theory]
	[InlineData(false, false, false)]
	[InlineData(false, false, true)]
	[InlineData(false, true, false)]
	[InlineData(false, true, true)]
	[InlineData(true, false, false)]
	[InlineData(true, false, true)]
	[InlineData(true, true, false)]
	[InlineData(true, true, true)]
	public async Task CreateRequest_OnlyDifferentTeamsShouldCreateTransferApprovals(
		bool differentTeam,
		bool hasCurrentMembership,
		bool autoApprove)
	{
		using var scope = this.factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ManagementHubDbContext>();
		await using var transaction = await db.Database.BeginTransactionAsync();
		var player = await db.Users.SingleAsync(user => user.Email == "referee@example.com");
		var origin = await db.Teams.SingleAsync(team => team.Name == "Yankees");
		var destination = differentTeam
			? await db.Teams.SingleAsync(team => team.Name == "BA Jacks")
			: origin;
		destination.AutoApprovePlayerRequests = autoApprove;
		db.RefereeTeams.RemoveRange(await db.RefereeTeams
			.Where(membership => membership.RefereeId == player.Id && membership.AssociationType == RefereeTeamAssociationType.Player)
			.ToListAsync());
		db.TeamInvitations.RemoveRange(await db.TeamInvitations
			.Where(invite => invite.Email == player.Email && invite.TeamId == destination.Id)
			.ToListAsync());
		var removedAt = DateTime.UtcNow;
		db.TeamPlayerActivities.Add(new TeamPlayerActivity
		{
			UserId = player.Id,
			TeamId = origin.Id,
			Email = player.Email,
			InitiatorUserId = player.Id,
			ActivityType = TeamPlayerActivityType.PlayerRemoved,
			CreatedAt = removedAt,
		});
		if (hasCurrentMembership)
		{
			db.RefereeTeams.Add(new RefereeTeam
			{
				RefereeId = player.Id,
				TeamId = origin.Id,
				AssociationType = RefereeTeamAssociationType.Player,
				CreatedAt = removedAt,
				UpdatedAt = removedAt,
			});
		}

		await db.SaveChangesAsync();
		var command = new CreateTeamInviteRequestCommand(db);
		var userId = player.UniqueId != null
			? UserIdentifier.Parse(player.UniqueId)
			: UserIdentifier.FromLegacyUserId(player.Id);
		var result = await command.CreateTeamInviteRequestAsync(
			new TeamIdentifier(destination.Id), player.Email, userId,
			RefereeTeamAssociationType.Player, CancellationToken.None);

		result.Code.Should().Be(autoApprove
			? ICreateTeamInviteRequestCommand.CreateResultCode.AutoApproved
			: ICreateTeamInviteRequestCommand.CreateResultCode.RequestCreated);
		var invitation = await db.TeamInvitations.SingleAsync(invite =>
			invite.Email == player.Email && invite.TeamId == destination.Id);
		var approvals = await db.NgbTransferApprovals
			.Where(approval => approval.TeamInvitationId == invitation.Id).ToListAsync();
		if (differentTeam)
		{
			invitation.OriginTeamId.Should().Be(origin.Id);
			approvals.Should().NotBeEmpty();
		}
		else
		{
			invitation.OriginTeamId.Should().BeNull();
			invitation.IsInternalTransfer.Should().BeNull();
			approvals.Should().BeEmpty();
			if (!autoApprove)
			{
				result.PendingNgbApprovals.Should().BeEmpty();
			}
		}

		invitation.AcceptedAt.HasValue.Should().Be(autoApprove);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task CleanupMigration_ShouldRemoveOnlySameTeamTransferApprovals(bool completed)
	{
		using var scope = this.factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ManagementHubDbContext>();
		await using var transaction = await db.Database.BeginTransactionAsync();
		var player = await db.Users.SingleAsync(user => user.Email == "referee@example.com");
		var origin = await db.Teams.SingleAsync(team => team.Name == "Yankees");
		var destination = await db.Teams.SingleAsync(team => team.Name == "BA Jacks");
		var ngb = await db.NationalGoverningBodies.SingleAsync(item => item.CountryCode == "USA");
		var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		var fixtureId = Guid.NewGuid().ToString("N");
		var invitations = new[] { (long?)origin.Id, destination.Id, null }.Select((originId, index) => new TeamInvitation
		{
			TeamId = origin.Id,
			OriginTeamId = originId,
			IsInternalTransfer = originId.HasValue ? true : null,
			Email = $"same-team-cleanup-{fixtureId}-{index}@example.test",
			InitiatorUserId = player.Id,
			CreatedAt = createdAt,
			AcceptedAt = completed ? createdAt : null,
		}).ToArray();
		var approvals = invitations.Select(invitation => new NgbTransferApproval
		{
			TeamInvitation = invitation,
			Ngb = ngb,
			CreatedAt = createdAt,
			ApprovedAt = completed ? createdAt : null,
		}).ToArray();
		db.NgbTransferApprovals.AddRange(approvals);
		await db.SaveChangesAsync();

		var migration = new RemoveSameTeamTransfers();
		db.Database.GetMigrations().Should().Contain("20261102090000_RemoveSameTeamTransfers");
		foreach (var operation in migration.UpOperations.Cast<SqlOperation>())
		{
			await db.Database.ExecuteSqlRawAsync(operation.Sql);
		}

		db.ChangeTracker.Clear();
		var ids = approvals.Select(approval => approval.Id).ToArray();
		var remaining = await db.NgbTransferApprovals.Where(approval => ids.Contains(approval.Id)).ToListAsync();
		remaining.Select(approval => approval.Id).Should().BeEquivalentTo(approvals.Skip(1).Select(approval => approval.Id));
		var cleaned = await db.TeamInvitations.SingleAsync(invite => invite.Id == invitations[0].Id);
		cleaned.OriginTeamId.Should().BeNull();
		cleaned.IsInternalTransfer.Should().BeNull();
		cleaned.AcceptedAt.Should().Be(invitations[0].AcceptedAt);
		var genuineTransfer = await db.TeamInvitations.SingleAsync(invite => invite.Id == invitations[1].Id);
		genuineTransfer.OriginTeamId.Should().Be(destination.Id);
		genuineTransfer.IsInternalTransfer.Should().BeTrue();
	}
}
