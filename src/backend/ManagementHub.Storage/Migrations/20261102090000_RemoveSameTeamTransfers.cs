using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <summary>
/// Removes transfer approvals for invitations whose origin and destination are the same team.
/// </summary>
[DbContext(typeof(ManagementHubDbContext))]
[Migration("20261102090000_RemoveSameTeamTransfers")]
public class RemoveSameTeamTransfers : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql(
			"""
			DELETE FROM ngb_transfer_approvals
			WHERE team_invitation_id IN (
				SELECT id FROM team_invitations WHERE origin_team_id = team_id
			);

			UPDATE team_invitations
			SET origin_team_id = NULL, is_internal_transfer = NULL
			WHERE origin_team_id = team_id;
			""");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		// Erroneous transfer approvals cannot be reconstructed after cleanup.
	}
}
