using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
/// <remarks>
/// This migration adds Description and ContactEmail columns to the teams table.
/// </remarks>
public partial class AddTeamLogoDescriptionContactEmail : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "description",
			table: "teams",
			type: "text",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "contact_email",
			table: "teams",
			type: "character varying",
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "description",
			table: "teams");

		migrationBuilder.DropColumn(
			name: "contact_email",
			table: "teams");
	}
}
