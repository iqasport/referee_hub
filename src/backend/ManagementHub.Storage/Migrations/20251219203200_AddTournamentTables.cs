using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
public partial class AddTournamentTables : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "tournaments",
			columns: table => new
			{
				id = table.Column<long>(type: "bigint", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				unique_id = table.Column<string>(type: "character varying", nullable: false),
				name = table.Column<string>(type: "character varying", nullable: false),
				description = table.Column<string>(type: "text", nullable: false),
				start_date = table.Column<DateOnly>(type: "date", nullable: false),
				end_date = table.Column<DateOnly>(type: "date", nullable: false),
				type = table.Column<int>(type: "integer", nullable: false),
				country = table.Column<string>(type: "character varying", nullable: false),
				city = table.Column<string>(type: "character varying", nullable: false),
				place = table.Column<string>(type: "character varying", nullable: true),
				organizer = table.Column<string>(type: "character varying", nullable: false),
				is_private = table.Column<bool>(type: "boolean", nullable: false),
				created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_tournaments", x => x.id);
			});

		migrationBuilder.CreateTable(
			name: "tournament_managers",
			columns: table => new
			{
				id = table.Column<long>(type: "bigint", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				tournament_id = table.Column<long>(type: "bigint", nullable: false),
				user_id = table.Column<long>(type: "bigint", nullable: false),
				created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_tournament_managers", x => x.id);
				table.ForeignKey(
					name: "fk_tournament_managers_tournament",
					column: x => x.tournament_id,
					principalTable: "tournaments",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "fk_tournament_managers_user",
					column: x => x.user_id,
					principalTable: "users",
					principalColumn: "id",
					onDelete: ReferentialAction.Restrict);
			});

		migrationBuilder.CreateIndex(
			name: "index_tournaments_on_unique_id",
			table: "tournaments",
			column: "unique_id",
			unique: true);

		migrationBuilder.CreateIndex(
			name: "index_tournament_managers_on_tournament_id_and_user_id",
			table: "tournament_managers",
			columns: new[] { "tournament_id", "user_id" },
			unique: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "tournament_managers");

		migrationBuilder.DropTable(
			name: "tournaments");
	}
}
