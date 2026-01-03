using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
public partial class Phase5AddTeamManagerAddedBy : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<long>(
			name: "added_by_user_id",
			table: "team_managers",
			type: "bigint",
			nullable: false,
			defaultValue: 0L);

		migrationBuilder.CreateIndex(
			name: "IX_team_managers_added_by_user_id",
			table: "team_managers",
			column: "added_by_user_id");

		migrationBuilder.AddForeignKey(
			name: "fk_team_managers_added_by_user",
			table: "team_managers",
			column: "added_by_user_id",
			principalTable: "users",
			principalColumn: "id",
			onDelete: ReferentialAction.Restrict);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_team_managers_added_by_user",
			table: "team_managers");

		migrationBuilder.DropIndex(
			name: "IX_team_managers_added_by_user_id",
			table: "team_managers");

		migrationBuilder.DropColumn(
			name: "added_by_user_id",
			table: "team_managers");
	}
}
