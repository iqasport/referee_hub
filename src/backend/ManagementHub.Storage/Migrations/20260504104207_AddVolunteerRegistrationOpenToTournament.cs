using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteerRegistrationOpenToTournament : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "observations",
                table: "tournament_invites",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_volunteer_registration_open",
                table: "tournaments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_volunteer_registration_open",
                table: "tournaments");

            migrationBuilder.DropColumn(
                name: "observations",
                table: "tournament_invites");
        }
    }
}
