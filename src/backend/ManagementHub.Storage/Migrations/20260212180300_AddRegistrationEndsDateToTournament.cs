using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
/// <remarks>
/// This migration adds the RegistrationEndsDate column to the tournaments table.
/// </remarks>
public partial class AddRegistrationEndsDateToTournament : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<DateOnly>(
			name: "registration_ends_date",
			table: "tournaments",
			type: "date",
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "registration_ends_date",
			table: "tournaments");
	}
}
