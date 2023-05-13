using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Add_UniqueId_to_TestAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "unique_id",
                table: "test_results",
                type: "varchar(40)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unique_id",
                table: "test_attempts",
                type: "varchar(40)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "unique_id",
                table: "test_results");

            migrationBuilder.DropColumn(
                name: "unique_id",
                table: "test_attempts");
        }
    }
}
