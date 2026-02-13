using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

/// <inheritdoc />
public partial class AddUserAttributesTable : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "user_attributes",
			columns: table => new
			{
				user_id = table.Column<long>(type: "bigint", nullable: false),
				key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
				attribute_value = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
				created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_user_attributes", x => new { x.user_id, x.key });
				table.ForeignKey(
					name: "fk_user_attributes_user_user_id",
					column: x => x.user_id,
					principalTable: "users",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
			});
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "user_attributes");
	}
}
