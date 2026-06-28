using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddFlagRunnerCertifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "flag_runner_referees_count",
                table: "national_governing_body_stats",
                type: "INTEGER",
                nullable: true,
                defaultValueSql: "0");

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

            migrationBuilder.DropColumn(
                name: "flag_runner_referees_count",
                table: "national_governing_body_stats");
        }
    }
}
