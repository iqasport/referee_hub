using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
public partial class AddTournamentInvitesAndParticipants : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "team_managers",
			columns: table => new
			{
				id = table.Column<long>(type: "bigint", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				team_id = table.Column<long>(type: "bigint", nullable: false),
				user_id = table.Column<long>(type: "bigint", nullable: false),
				created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_team_managers", x => x.id);
				table.ForeignKey(
					name: "fk_team_managers_team",
					column: x => x.team_id,
					principalTable: "teams",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "fk_team_managers_user",
					column: x => x.user_id,
					principalTable: "users",
					principalColumn: "id",
					onDelete: ReferentialAction.Restrict);
			});

		migrationBuilder.CreateTable(
			name: "tournament_invites",
			columns: table => new
			{
				id = table.Column<long>(type: "bigint", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				tournament_id = table.Column<long>(type: "bigint", nullable: false),
				participant_type = table.Column<string>(type: "character varying", nullable: false),
				participant_id = table.Column<string>(type: "character varying", nullable: false),
				initiator_user_id = table.Column<long>(type: "bigint", nullable: false),
				created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				tournament_manager_approval = table.Column<int>(type: "integer", nullable: false),
				tournament_manager_approval_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
				participant_approval = table.Column<int>(type: "integer", nullable: false),
				participant_approval_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_tournament_invites", x => x.id);
				table.ForeignKey(
					name: "fk_tournament_invites_initiator",
					column: x => x.initiator_user_id,
					principalTable: "users",
					principalColumn: "id",
					onDelete: ReferentialAction.Restrict);
				table.ForeignKey(
					name: "fk_tournament_invites_tournament",
					column: x => x.tournament_id,
					principalTable: "tournaments",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateTable(
			name: "tournament_team_participants",
			columns: table => new
			{
				id = table.Column<long>(type: "bigint", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				tournament_id = table.Column<long>(type: "bigint", nullable: false),
				team_id = table.Column<long>(type: "bigint", nullable: false),
				team_name = table.Column<string>(type: "character varying", nullable: false),
				created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_tournament_team_participants", x => x.id);
				table.ForeignKey(
					name: "fk_tournament_team_participants_team",
					column: x => x.team_id,
					principalTable: "teams",
					principalColumn: "id",
					onDelete: ReferentialAction.Restrict);
				table.ForeignKey(
					name: "fk_tournament_team_participants_tournament",
					column: x => x.tournament_id,
					principalTable: "tournaments",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "index_team_managers_on_team_id_and_user_id",
			table: "team_managers",
			columns: new[] { "team_id", "user_id" },
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_team_managers_user_id",
			table: "team_managers",
			column: "user_id");

		migrationBuilder.CreateIndex(
			name: "index_tournament_invites_on_tournament_and_participant",
			table: "tournament_invites",
			columns: new[] { "tournament_id", "participant_type", "participant_id" },
			unique: true,
			filter: "tournament_manager_approval != 2 AND participant_approval != 2");

		migrationBuilder.CreateIndex(
			name: "IX_tournament_invites_initiator_user_id",
			table: "tournament_invites",
			column: "initiator_user_id");

		migrationBuilder.CreateIndex(
			name: "index_tournament_team_participants_on_tournament_and_team",
			table: "tournament_team_participants",
			columns: new[] { "tournament_id", "team_id" },
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_tournament_team_participants_team_id",
			table: "tournament_team_participants",
			column: "team_id");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "team_managers");

		migrationBuilder.DropTable(
			name: "tournament_invites");

		migrationBuilder.DropTable(
			name: "tournament_team_participants");
	}
}
