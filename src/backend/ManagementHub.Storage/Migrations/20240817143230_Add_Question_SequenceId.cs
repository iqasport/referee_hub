using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementHub.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Add_Question_SequenceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "seq_id",
                table: "questions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

			migrationBuilder.Sql(@"
                WITH SeqIdWithinTest AS (
                    SELECT
                        id,
                        ROW_NUMBER() OVER (PARTITION BY test_id ORDER BY id) AS RowNumber
                    FROM
                        questions
                )
                UPDATE questions q
                SET seq_id = s.RowNumber
                FROM SeqIdWithinTest s
                WHERE q.id = s.id;
			");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "seq_id",
                table: "questions");
        }
    }
}
