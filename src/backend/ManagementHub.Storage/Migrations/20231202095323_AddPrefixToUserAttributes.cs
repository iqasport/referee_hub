using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddPrefixToUserAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_attributes",
                table: "user_attributes");

            migrationBuilder.AddColumn<string>(
                name: "prefix",
                table: "user_attributes",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_attributes",
                table: "user_attributes",
                columns: new[] { "user_id", "prefix", "key" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_attributes",
                table: "user_attributes");

            migrationBuilder.DropColumn(
                name: "prefix",
                table: "user_attributes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_attributes",
                table: "user_attributes",
                columns: new[] { "user_id", "key" });
        }
    }
}
