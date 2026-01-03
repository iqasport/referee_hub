using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
public partial class AddPhase4TournamentRosters : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "tournament_team_roster_entries",
			columns: table => new
			{
				id = table.Column<long>(type: "bigint", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				tournament_team_participant_id = table.Column<long>(type: "bigint", nullable: false),
				user_id = table.Column<long>(type: "bigint", nullable: false),
				role = table.Column<int>(type: "integer", nullable: false),
				jersey_number = table.Column<string>(type: "character varying", maxLength: 5, nullable: true),
				created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_tournament_team_roster_entries", x => x.id);
				table.CheckConstraint("chk_roster_entry_jersey_number", "role != 0 OR (role = 0 AND jersey_number IS NOT NULL)");
				table.ForeignKey(
					name: "fk_tournament_team_roster_entries_participant",
					column: x => x.tournament_team_participant_id,
					principalTable: "tournament_team_participants",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "fk_tournament_team_roster_entries_user",
					column: x => x.user_id,
					principalTable: "users",
					principalColumn: "id",
					onDelete: ReferentialAction.Restrict);
			});

		migrationBuilder.CreateTable(
			name: "user_delicate_info",
			columns: table => new
			{
				id = table.Column<long>(type: "bigint", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				user_id = table.Column<long>(type: "bigint", nullable: false),
				gender = table.Column<string>(type: "character varying", nullable: true),
				updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_user_delicate_info", x => x.id);
				table.ForeignKey(
					name: "fk_user_delicate_info_user",
					column: x => x.user_id,
					principalTable: "users",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "index_roster_entries_on_participant_and_user",
			table: "tournament_team_roster_entries",
			columns: new[] { "tournament_team_participant_id", "user_id" },
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_tournament_team_roster_entries_user_id",
			table: "tournament_team_roster_entries",
			column: "user_id");

		migrationBuilder.CreateIndex(
			name: "index_user_delicate_info_on_user_id",
			table: "user_delicate_info",
			column: "user_id",
			unique: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "tournament_team_roster_entries");

		migrationBuilder.DropTable(
			name: "user_delicate_info");
	}
}
