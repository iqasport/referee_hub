using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
/// <remarks>
/// This migration adds the observations column to the tournament_invites table
/// to store free-text notes submitted by players during individual registration.
/// </remarks>
public partial class AddObservationsToTournamentInvite : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "observations",
			table: "tournament_invites",
			type: "character varying(1000)",
			maxLength: 1000,
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "observations",
			table: "tournament_invites");
	}
}
