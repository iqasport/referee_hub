using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
/// <remarks>
/// This migration adds AllowsIndividualRegistration and AllowsTeamRegistration columns to the tournaments table
/// to support Fantasy tournament registration type configuration.
/// </remarks>
public partial class AddAllowsIndividualRegistrationToTournament : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<bool>(
				name: "allows_individual_registration",
				table: "tournaments",
				type: "boolean",
				nullable: false,
				defaultValue: false);

		migrationBuilder.AddColumn<bool>(
				name: "allows_team_registration",
				table: "tournaments",
				type: "boolean",
				nullable: false,
				defaultValue: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
				name: "allows_individual_registration",
				table: "tournaments");

		migrationBuilder.DropColumn(
				name: "allows_team_registration",
				table: "tournaments");
	}
}
