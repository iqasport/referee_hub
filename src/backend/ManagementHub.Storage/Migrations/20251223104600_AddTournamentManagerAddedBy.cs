using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
public partial class AddTournamentManagerAddedBy : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<long>(
			name: "added_by_user_id",
			table: "tournament_managers",
			type: "bigint",
			nullable: false,
			defaultValue: 0L);

		migrationBuilder.AddForeignKey(
			name: "fk_tournament_managers_added_by_user",
			table: "tournament_managers",
			column: "added_by_user_id",
			principalTable: "users",
			principalColumn: "id",
			onDelete: ReferentialAction.Restrict);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_tournament_managers_added_by_user",
			table: "tournament_managers");

		migrationBuilder.DropColumn(
			name: "added_by_user_id",
			table: "tournament_managers");
	}
}
