using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
/// <remarks>
/// This migration adds the IsRegistrationOpen column to the tournaments table.
/// </remarks>
public partial class AddIsRegistrationOpenToTournament : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<bool>(
				name: "is_registration_open",
				table: "tournaments",
				type: "boolean",
				nullable: false,
				defaultValue: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
				name: "is_registration_open",
				table: "tournaments");
	}
}
