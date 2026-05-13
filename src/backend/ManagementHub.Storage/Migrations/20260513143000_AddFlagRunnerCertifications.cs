using ManagementHub.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations;

[DbContext(typeof(ManagementHubDbContext))]
[Migration("20260513143000_AddFlagRunnerCertifications")]
public partial class AddFlagRunnerCertifications : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql(
			"""
			INSERT INTO certifications (created_at, updated_at, level, version)
			SELECT NOW(), NOW(), 5, v.version
			FROM (VALUES (0), (1), (2), (3)) AS v(version)
			WHERE NOT EXISTS (
				SELECT 1
				FROM certifications c
				WHERE c.level = 5 AND c.version = v.version
			);
			""");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql("DELETE FROM certifications WHERE level = 5;");
	}
}
