using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
/// <remarks>
/// Adds the AllowsIndividualRegistration column to the tournaments table.
/// Only relevant for Fantasy tournaments; defaults to false for existing rows.
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
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "allows_individual_registration",
			table: "tournaments");
	}
}
