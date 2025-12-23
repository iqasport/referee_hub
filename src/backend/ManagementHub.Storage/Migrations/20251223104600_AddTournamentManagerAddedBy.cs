using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
public partial class AddTournamentManagerAddedBy : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// Add the column as nullable first
		migrationBuilder.AddColumn<long>(
			name: "added_by_user_id",
			table: "tournament_managers",
			type: "bigint",
			nullable: true);

		// For existing records, set added_by_user_id to user_id (they added themselves)
		migrationBuilder.Sql(
			"UPDATE tournament_managers SET added_by_user_id = user_id WHERE added_by_user_id IS NULL");

		// Make the column non-nullable
		migrationBuilder.AlterColumn<long>(
			name: "added_by_user_id",
			table: "tournament_managers",
			type: "bigint",
			nullable: false);

		// Add the foreign key
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
