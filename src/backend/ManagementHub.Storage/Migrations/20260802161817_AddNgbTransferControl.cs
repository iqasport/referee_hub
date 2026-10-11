using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ManagementHub.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddNgbTransferControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_internal_transfer",
                table: "team_invitations",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "origin_team_id",
                table: "team_invitations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "auto_approve_internal_transfers",
                table: "national_governing_bodies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ngb_transfer_approvals",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        .Annotation("Sqlite:Autoincrement", true),
                    team_invitation_id = table.Column<long>(type: "INTEGER", nullable: false),
                    ngb_id = table.Column<long>(type: "INTEGER", nullable: false),
                    is_origin_ngb = table.Column<bool>(type: "boolean", nullable: false),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewed_by_user_id = table.Column<long>(type: "INTEGER", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ngb_transfer_approvals", x => x.id);
                    table.ForeignKey(
                        name: "fk_ngb_transfer_approvals_invitation",
                        column: x => x.team_invitation_id,
                        principalTable: "team_invitations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ngb_transfer_approvals_ngb",
                        column: x => x.ngb_id,
                        principalTable: "national_governing_bodies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ngb_transfer_approvals_reviewer",
                        column: x => x.reviewed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_team_invitations_origin_team_id",
                table: "team_invitations",
                column: "origin_team_id");

            migrationBuilder.CreateIndex(
                name: "index_ngb_transfer_approvals_on_invitation_and_ngb",
                table: "ngb_transfer_approvals",
                columns: new[] { "team_invitation_id", "ngb_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_ngb_transfer_approvals_on_ngb_id",
                table: "ngb_transfer_approvals",
                column: "ngb_id");

            migrationBuilder.CreateIndex(
                name: "IX_ngb_transfer_approvals_reviewed_by_user_id",
                table: "ngb_transfer_approvals",
                column: "reviewed_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_team_invitations_origin_team",
                table: "team_invitations",
                column: "origin_team_id",
                principalTable: "teams",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_team_invitations_origin_team",
                table: "team_invitations");

            migrationBuilder.DropTable(
                name: "ngb_transfer_approvals");

            migrationBuilder.DropIndex(
                name: "IX_team_invitations_origin_team_id",
                table: "team_invitations");

            migrationBuilder.DropColumn(
                name: "is_internal_transfer",
                table: "team_invitations");

            migrationBuilder.DropColumn(
                name: "origin_team_id",
                table: "team_invitations");

            migrationBuilder.DropColumn(
                name: "auto_approve_internal_transfers",
                table: "national_governing_bodies");
        }
    }
}
